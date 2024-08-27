using System;

namespace Scenebox;

public sealed class PropHelper : Component, Component.ICollisionListener
{
    Prop Prop;

    [Sync] public Guid CreatorId { get; set; } = Guid.Empty;
    [Sync] public float Health { get; set; } = 1;
    [Sync] NetDictionary<int, BodyInfo> NetworkedBodies { get; set; } = new();
    [Sync, Change( "InitCloudModel" )] string CloudModel { get; set; } = "";

    Vector3 _lastPosition = Vector3.Zero;
    Vector3 Velocity;
    ModelPhysics Physics;
    Rigidbody Rigidbody;

    struct BodyInfo
    {
        public PhysicsBodyType Type;
        public Transform Transform;
    }

    protected override void OnStart()
    {
        Prop = Components.Get<Prop>();
        Health = Prop?.Health ?? 0;
        Physics = Components.Get<ModelPhysics>( FindMode.EverythingInSelf );
        Rigidbody = Components.Get<Rigidbody>();
        _lastPosition = Prop?.Transform?.Position ?? Transform.Position;
        Velocity = 0;

        InitCloudModel();
    }

    [Broadcast]
    public void Damage( float amount )
    {
        if ( (Prop?.Health ?? 0) <= 0 ) return;
        if ( IsProxy ) return;

        Health -= amount;
        if ( Health <= 0 )
        {
            Kill();
        }
    }

    public void Kill()
    {
        if ( IsProxy ) return;

        var gibs = Prop?.CreateGibs();
        foreach ( var gib in gibs )
        {
            gib.GameObject.NetworkSpawn();
            gib.GameObject.Network.SetOrphanedMode( NetworkOrphaned.Host );
        }

        bool isExpodingProp = IsExpodingProp();
        Vector3 position = Prop.Transform.Position;
        GameObject.DestroyImmediate();
        if ( isExpodingProp )
        {
            GameManager.Instance.SpawnExplosion( position, 500, 80_000 );
        }
    }

    public void AddForce( int bodyIndex, Vector3 force )
    {
        if ( IsProxy ) return;

        var body = Physics?.PhysicsGroup?.GetBody( bodyIndex );
        if ( body.IsValid() )
            body.ApplyForce( force );
        else if ( bodyIndex == 0 && Rigidbody.IsValid() )
            Rigidbody.Velocity += force / Rigidbody.PhysicsBody.Mass;
    }

    public async void AddDamagingForce( Vector3 force, float damage )
    {
        if ( IsProxy ) return;

        if ( Physics.IsValid() )
        {
            foreach ( var body in Physics.PhysicsGroup.Bodies )
            {
                AddForce( body.GroupIndex, force );
            }
        }
        else
        {
            AddForce( 0, force );
        }

        await GameTask.DelaySeconds( 1f / Scene.FixedUpdateFrequency + 0.05f );

        Damage( damage );
    }

    [Broadcast]
    public void BroadcastAddForce( int bodyIndex, Vector3 force )
    {
        if ( IsProxy ) return;

        AddForce( bodyIndex, force );
    }

    [Broadcast]
    public void BroadcastAddDamagingForce( Vector3 force, float damage )
    {
        if ( IsProxy ) return;

        AddDamagingForce( force, damage );
    }


    protected override void OnFixedUpdate()
    {
        if ( Prop.IsValid() )
        {
            Velocity = (Prop.Transform.Position - _lastPosition) / Time.Delta;
            _lastPosition = Prop.Transform.Position;
        }

        UpdateNetworkedBodies();
    }

    void UpdateNetworkedBodies()
    {
        if ( !Physics.IsValid() )
        {
            Physics = Components.Get<ModelPhysics>( FindMode.EverythingInSelf );
            Rigidbody = Components.Get<Rigidbody>();
            return;
        }

        if ( !Network.IsOwner )
        {
            var rootBody = FindRootBody();

            foreach ( var (groupId, info) in NetworkedBodies )
            {
                var group = Physics.PhysicsGroup.GetBody( groupId );
                group.Transform = info.Transform;
                group.BodyType = info.Type;
            }

            if ( rootBody.IsValid() )
            {
                rootBody.Transform = Physics.Renderer.GameObject.Transform.World;
            }

            return;
        }

        foreach ( var body in Physics.PhysicsGroup.Bodies )
        {
            if ( body.GroupIndex == 0 ) continue;

            var tx = body.GetLerpedTransform( Time.Now );
            NetworkedBodies[body.GroupIndex] = new BodyInfo
            {
                Type = body.BodyType,
                Transform = tx
            };
        }
    }

    PhysicsBody FindRootBody()
    {
        var body = Physics.PhysicsGroup.Bodies.FirstOrDefault();
        if ( body == null ) return null;
        while ( body.Parent.IsValid() )
        {
            body = body.Parent;
        }
        return body;
    }

    public void OnCollisionStart( Collision other )
    {
        if ( IsProxy ) return;

        var speed = Velocity.Length;
        var otherSpeed = other.Other.Body.Velocity.Length;
        if ( otherSpeed > speed ) speed = otherSpeed;
        if ( speed >= 1200 )
        {
            var dmg = speed / 8f;
            Damage( dmg );

            if ( other.Other.GameObject.Root.Components.TryGet<Player>( out var player ) )
            {
                player.Damage( dmg, 1 );
            }
        }
    }


    async void InitCloudModel()
    {
        if ( string.IsNullOrWhiteSpace( CloudModel ) ) return;

        var package = await Package.Fetch( CloudModel, false );
        await package.MountAsync();

        var model = Model.Load( package.GetMeta( "PrimaryAsset", "" ) );
        if ( model is null ) return;
        if ( Prop.IsValid() )
        {
            Prop.Enabled = false;
            Prop.Model = model;
            Prop.Enabled = true;
        }

        if ( this?.IsValid ?? false )
        {
            var renderer = Components.Get<Renderer>();
            if ( renderer?.IsValid ?? false )
            {
                renderer.Enabled = true;
            }
        }
    }

    [Broadcast]
    public void SetCloudModel( string cloudModel )
    {
        CloudModel = cloudModel;

        var renderer = Components.Get<Renderer>();
        if ( renderer.IsValid() )
        {
            renderer.Enabled = false;
        }

        InitCloudModel();
    }

    bool IsExpodingProp()
    {
        return Prop?.Model?.ResourceName?.Contains( "explosive" ) ?? false;
    }
}