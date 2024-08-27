
using System;

namespace Scenebox;

public class Gravgun : Weapon
{

    [Property, Group( "Sounds" )] SoundEvent GrabSound { get; set; }
    [Property, Group( "Sounds" )] SoundEvent ThrowSound { get; set; }
    [Property, Group( "Sounds" )] SoundEvent DropSound { get; set; }
    [Property, Group( "Sounds" )] SoundEvent LookAtSound { get; set; }
    [Property, Group( "Sounds" )] SoundEvent LookAwaySound { get; set; }
    [Property, Group( "Sounds" )] SoundEvent CantPickupSound { get; set; }
    [Property, Group( "Sounds" )] SoundEvent DryFireSound { get; set; }
    [Property, Group( "Sounds" )] SoundEvent HoldingSound { get; set; }

    bool CanPickup = false;
    bool CouldPickup = false;
    TimeSince timeSinceLastCanPickup = 10f;

    protected virtual float MaxPullDistance => 2000f;
    protected virtual float MaxPushDistance => 500;
    protected virtual float LinearFrequency => 10f;
    protected virtual float LinearDampingRatio => 1f;
    protected virtual float AngularFrequency => 10f;
    protected virtual float AngularDampingRatio => 1f;
    protected virtual float PullForce => 20f;
    protected virtual float PushForce => 1000f;
    protected virtual float ThrowForce => 2000f;
    protected virtual float HoldDistance => 50f;
    protected virtual float DropCooldown => 0.5f;
    protected virtual float BreakLinearForce => 2000f;

    public const string GrabbedTag = "grabbed";

    public Vector3 HeldPosition { get; private set; }
    public Rotation HeldRotation { get; private set; }
    public Vector3 HoldPosition { get; private set; }
    public Rotation HoldRotation { get; private set; }

    [Sync] public Guid GrabbedObjectId { get; set; }
    [Sync] public int GrabbedBone { get; set; }
    PhysicsBody HeldBody = null;
    public GameObject GrabbedObject => (GrabbedObjectId == Guid.Empty) ? null : Scene.Directory.FindByGuid( GrabbedObjectId );
    SoundHandle HoldingSoundHandle;

    SceneTrace GravGunTrace => Scene.Trace.Ray( new Ray( Player.Head.Transform.Position, Player.Direction.Forward ), 350f )
            .IgnoreGameObjectHierarchy( GameObject.Root )
            .WithoutTags( "trigger" );

    public override void Update()
    {
        if ( !IsEquipped ) return;

        if ( Input.Pressed( "attack1" ) )
        {
            PrimaryUse();
        }
        else if ( Input.Pressed( "attack2" ) )
        {
            SecondaryUse();
        }

        GrabMove( Player.Head.Transform.Position, Player.Direction.Forward, Player.Head.Transform.Rotation );
        PhysicsStep();
    }

    public override void FixedUpdate()
    {
        if ( !IsEquipped ) return;

        var tr = GravGunTrace.Run();

        if ( GrabbedObject.IsValid() )
        {
            CanPickup = false;

            if ( !(HoldingSoundHandle?.IsPlaying ?? false) )
            {
                HoldingSoundHandle = Sound.Play( HoldingSound, Transform.Position );
                if ( Network.IsOwner )
                {
                    HoldingSoundHandle.Volume = 0.4f;
                    HoldingSoundHandle.ListenLocal = true;
                }
            }
            else
            {
                HoldingSoundHandle.Position = Transform.Position;
            }
        }
        else
        {
            CanPickup = tr.Body.IsValid() && tr.Body.BodyType == PhysicsBodyType.Dynamic && !tr.GameObject.Root.Tags.HasAny( "map", "player" );
            if ( CanPickup ) timeSinceLastCanPickup = 0f;

            HoldingSoundHandle?.Stop();
        }

        if ( CanPickup && !CouldPickup )
        {
            CouldPickup = true;
            BroadcastSound( 3 );
        }
        else if ( CouldPickup && !CanPickup )
        {
            if ( timeSinceLastCanPickup > 1f )
            {
                CouldPickup = false;
                BroadcastSound( 4 );
            }
        }

        Player?.ViewModel?.ModelRenderer?.Set( "b_empty", CouldPickup );

    }

    protected override void OnDestroy()
    {
        HoldingSoundHandle?.Stop();
        GrabEnd();
    }

    protected override void OnUnequip()
    {
        base.OnUnequip();

        HoldingSoundHandle?.Stop();
        GrabEnd();
    }

    void PrimaryUse()
    {
        BroadcastUseEffects();

        if ( GrabbedObject.IsValid() )
        {
            GrabEnd();
            CanPickup = true;
        }

        if ( CanPickup )
        {
            var tr = GravGunTrace.Run();

            if ( tr.Body.IsValid() )
            {
                tr.GameObject.Network.TakeOwnership();
                tr.Body.Velocity += Player.Direction.Forward * ThrowForce;
            }
            BroadcastSound( 1 );
        }
        else
        {
            BroadcastSound( 6 );
            timeSinceLastCanPickup = 10f;
        }
    }

    void SecondaryUse()
    {
        BroadcastUseEffects();

        if ( CanPickup )
        {
            BroadcastSound( 0 );

            var tr = GravGunTrace.Run();

            if ( tr.Body.IsValid() && !tr.GameObject.Root.Tags.HasAny( "map", "player", GrabbedTag ) )
            {
                GrabInit( tr.GameObject, tr.Body, Player.Head.Transform.Position + Player.Direction.Forward * HoldDistance, Player.Direction );
            }
        }
        else
        {
            if ( GrabbedObject.IsValid() )
            {
                GrabEnd();
                BroadcastSound( 2 );
            }
            else
            {
                BroadcastSound( 5 );
                timeSinceLastCanPickup = 10f;
            }
        }
    }

    void PhysicsStep()
    {
        if ( !HeldBody.IsValid() ) return;

        var velocity = HeldBody.Velocity;
        Vector3.SmoothDamp( HeldBody.Position, HoldPosition, ref velocity, 0.075f, Time.Delta );
        HeldBody.Velocity = velocity;

        var angularVelocity = HeldBody.AngularVelocity;
        Rotation.SmoothDamp( HeldBody.Rotation, HoldRotation, ref angularVelocity, 0.075f, Time.Delta );
        HeldBody.AngularVelocity = angularVelocity;
    }

    void GrabInit( GameObject gameObject, PhysicsBody body, Vector3 grabPosition, Rotation grabRotation )
    {
        GrabbedObjectId = gameObject.Id;
        GameManager.Instance.BroadcastAddTag( gameObject.Id, GrabbedTag );
        gameObject.Network.TakeOwnership();
        GrabbedBone = body.GroupIndex;

        HeldBody = body;
        HeldPosition = HeldBody.LocalMassCenter;
        HeldRotation = grabRotation.Inverse * HeldBody.Rotation;

        HoldPosition = HeldBody.Position;
        HoldRotation = HeldBody.Rotation;

        HeldBody.Sleeping = false;
        HeldBody.AutoSleep = false;
    }

    void GrabMove( Vector3 startPos, Vector3 dir, Rotation rot )
    {
        if ( HeldBody.IsValid() )
        {
            var attachPos = HeldBody.FindClosestPoint( startPos );
            var holdDistance = HoldDistance + attachPos.Distance( HeldBody.MassCenter );

            HoldPosition = startPos - HeldPosition * HeldBody.Rotation + dir * holdDistance;
            HoldRotation = rot * HeldRotation;
        }
    }

    void GrabEnd()
    {
        if ( !GrabbedObject.IsValid() ) return;

        GameManager.Instance.BroadcastRemoveTag( GrabbedObjectId, GrabbedTag );
        GrabbedObjectId = Guid.Empty;
        HeldBody = null;
    }

    [Broadcast]
    void BroadcastUseEffects()
    {
        var playerRenderer = Player?.Body?.Components?.Get<SkinnedModelRenderer>();
        playerRenderer?.Set( "b_attack", true );
        Player?.ViewModel?.ModelRenderer?.Set( "b_attack", true );
    }

    [Broadcast]
    void BroadcastSound( int state )
    {
        SoundHandle sound;
        switch ( state )
        {
            case 0:
                sound = Sound.Play( GrabSound, Transform.Position );
                break;
            case 1:
                sound = Sound.Play( ThrowSound, Transform.Position );
                break;
            case 2:
                sound = Sound.Play( DropSound, Transform.Position );
                break;
            case 3:
                sound = Sound.Play( LookAtSound, Transform.Position );
                break;
            case 4:
                sound = Sound.Play( LookAwaySound, Transform.Position );
                break;
            case 5:
                sound = Sound.Play( CantPickupSound, Transform.Position );
                break;
            case 6:
                sound = Sound.Play( DryFireSound, Transform.Position );
                break;
            default:
                sound = Sound.Play( GrabSound, Transform.Position );
                break;
        }

        if ( Network.IsOwner )
        {
            sound.Volume = 0.3f;
            sound.ListenLocal = true;
        }
    }

}