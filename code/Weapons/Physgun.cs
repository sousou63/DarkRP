
using System;

namespace Scenebox;

public class Physgun : Weapon
{
    [Property] LineRenderer BeamParticles { get; set; }
    [Property, Group( "Prefabs" )] public GameObject FreezeParticles { get; set; }

    public PhysicsBody HeldBody { get; private set; }
    public Vector3 HeldPosition { get; private set; }
    public Rotation HeldRotation { get; private set; }
    public Vector3 HoldPosition { get; private set; }
    public Rotation HoldRotation { get; private set; }
    public float HoldDistance { get; private set; }
    public bool Grabbing { get; private set; }

    protected virtual float MinTargetDistance => 0f;
    protected virtual float MaxTargetDistance => 10_000f;
    protected virtual float LinearFrequency => 20f;
    protected virtual float LinearDampingRatio => 1.0f;
    protected virtual float AngularFrequency => 20f;
    protected virtual float AngularDampingRatio => 1.0f;
    protected virtual float TargetDistanceSpeed => 25.0f;
    protected virtual float RotateSpeed => 0.25f;
    protected virtual float RotateSnapAt => 45f;

    public const string GrabbedTag = "grabbed";

    [Sync] public bool BeamActive { get; set; } = false;
    [Sync] public Guid GrabbedObjectId { get; set; }
    [Sync] public int GrabbedBone { get; set; }
    [Sync] public Vector3 GrabbedPosition { get; set; }
    [Sync] Vector3 lastBeamPosition { get; set; } = Vector3.Zero;

    public GameObject GrabbedObject => (GrabbedObjectId == Guid.Empty) ? null : Scene.Directory.FindByGuid( GrabbedObjectId );

    protected override void OnUpdate()
    {
        base.OnUpdate();

        if ( BeamActive && IsEquipped )
        {
            BeamParticles.Enabled = true;
            UpdateBeam();
        }
        else
        {
            BeamParticles.Enabled = false;
        }
    }

    public override void Update()
    {

        if ( !IsEquipped ) return;
        if ( IsProxy ) return;

        var eyePos = Player.Head.Transform.Position;
        var eyeDir = Player.Direction.Forward;
        var eyeRot = Rotation.From( 0, Player.Direction.yaw, 0 );

        if ( Input.Pressed( "attack1" ) )
        {
            Player.BroadcastAttackAnimation();
            if ( !Grabbing ) Grabbing = true;
        }

        bool grabEnabled = Grabbing && Input.Down( "attack1" );
        bool wantsToFreeze = Input.Pressed( "attack2" );
        var grabbedObject = GrabbedObject;

        if ( grabbedObject.IsValid() && wantsToFreeze )
        {
            Player.BroadcastAttackAnimation();
        }

        BeamActive = grabEnabled;

        if ( grabEnabled )
        {
            if ( HeldBody.IsValid() )
            {
                UpdateGrab( eyePos, eyeRot, eyeDir, wantsToFreeze );
            }
            else
            {
                TryStartGrab( eyePos, eyeRot, eyeDir );
            }
        }
        else if ( Grabbing )
        {
            GrabEnd();
        }

        if ( !Grabbing && Input.Pressed( "reload" ) )
        {
            TryUnfreezeAll( eyePos, eyeRot, eyeDir );
        }

        if ( BeamActive )
        {
            Input.MouseWheel = 0;
        }

        PhysicsStep();
    }

    protected override void OnDestroy()
    {
        BeamParticles.Enabled = false;
        GrabEnd();
    }

    protected override void OnUnequip()
    {
        base.OnUnequip();

        GrabEnd();
    }

    private void TryUnfreezeAll( Vector3 eyePos, Rotation eyeRot, Vector3 eyeDir )
    {
        var tr = Scene.Trace.Ray( eyePos, eyePos + eyeDir * MaxTargetDistance )
            .UseHitboxes()
            .IgnoreGameObjectHierarchy( GameObject.Root )
            .WithoutTags( "player", "trigger" )
            .Run();

        if ( !tr.Hit || !tr.Body.IsValid() ) return;

        var rootObject = tr.GameObject;
        if ( !rootObject.IsValid() ) return;
        if ( rootObject.Tags.Has( "map" ) ) return;
        if ( rootObject.Root.Tags.Has( GrabbedTag ) ) return;

        var physicsGroup = tr.Body.PhysicsGroup;
        if ( physicsGroup == null ) return;

        bool unfrozen = false;

        rootObject.Root.Network.TakeOwnership();

        for ( int i = 0; i < physicsGroup.BodyCount; i++ )
        {
            var body = physicsGroup.GetBody( i );
            if ( !body.IsValid() ) continue;

            if ( body.BodyType != PhysicsBodyType.Dynamic )
            {
                body.BodyType = PhysicsBodyType.Dynamic;
                unfrozen = true;
            }
        }

        if ( unfrozen )
        {
            // TODO: Create unfreeze particle here
        }
    }

    private void TryStartGrab( Vector3 eyePos, Rotation eyeRot, Vector3 eyeDir )
    {
        var tr = Scene.Trace.Ray( eyePos, eyePos + eyeDir * MaxTargetDistance )
            .UseHitboxes()
            .IgnoreGameObjectHierarchy( GameObject.Root )
            .WithoutTags( "player", "trigger" )
            .Run();

        lastBeamPosition = tr.Hit ? tr.HitPosition : tr.EndPosition;
        if ( !tr.Hit || !tr.GameObject.IsValid() || tr.GameObject.Tags.Has( "map" ) || tr.StartedSolid ) return;

        var rootObject = tr.GameObject.Root;
        var body = tr.Body;

        if ( !body.IsValid() )
        {
            if ( rootObject.IsValid() )
            {
                var rigidbody = rootObject.Components.Get<Rigidbody>( FindMode.EverythingInSelfAndDescendants );
                if ( rigidbody.IsValid() )
                {
                    body = rigidbody.PhysicsBody;
                }
            }
        }

        if ( !body.IsValid() ) return;

        // Don't move keyframed unless it's a player
        if ( body.BodyType == PhysicsBodyType.Keyframed && !rootObject.Tags.Has( "player" ) ) return;

        // Unfreeze
        if ( body.BodyType != PhysicsBodyType.Dynamic )
        {
            body.BodyType = PhysicsBodyType.Dynamic;
            body.Sleeping = false;
        }

        if ( rootObject.Tags.Has( GrabbedTag ) ) return;

        GrabInit( body, eyePos, tr.EndPosition, eyeRot );

        GrabbedObjectId = tr.GameObject.Id;
        GameManager.Instance.BroadcastAddTag( GrabbedObjectId, GrabbedTag );
        GameManager.Instance.BroadcastAddHighlight( GrabbedObjectId, Color.Lerp( Color.Cyan, Color.White, 0.2f ), Color.Transparent, 0.3f );
        tr.GameObject.Network.TakeOwnership();

        GrabbedPosition = body.Transform.PointToLocal( tr.EndPosition );
        GrabbedBone = body.GroupIndex;
    }

    void UpdateGrab( Vector3 eyePos, Rotation eyeRot, Vector3 eyeDir, bool wantsToFreeze )
    {
        if ( wantsToFreeze )
        {
            if ( HeldBody.BodyType == PhysicsBodyType.Dynamic )
            {
                HeldBody.BodyType = PhysicsBodyType.Static;
                HeldBody.Velocity = 0;
                HeldBody.AngularVelocity = 0;
            }

            BroadcastFreezeParticles( HeldBody.IsValid() ? (HeldBody.Position + HeldPosition * HeldBody.Rotation) : lastBeamPosition );

            GrabEnd();
            return;
        }

        MoveTargetDistance( Input.MouseWheel.y * TargetDistanceSpeed );

        bool rotating = Input.Down( "use" );
        bool snapping = false;

        if ( rotating )
        {
            DoRotate( eyeRot, Input.MouseDelta * RotateSpeed );
            snapping = Input.Down( "run" );
            Player.CanMoveHead = false;
        }

        GrabMove( eyePos, eyeDir, eyeRot, snapping );
    }

    void GrabInit( PhysicsBody body, Vector3 startPosition, Vector3 grabPosition, Rotation rot )
    {
        if ( !body.IsValid() ) return;

        GrabEnd();

        Grabbing = true;
        HeldBody = body;
        HoldDistance = Vector3.DistanceBetween( startPosition, grabPosition );
        HoldDistance = HoldDistance.Clamp( MinTargetDistance, MaxTargetDistance );

        HeldRotation = rot.Inverse * HeldBody.Rotation;
        HeldPosition = HeldBody.Transform.PointToLocal( grabPosition );

        HoldPosition = HeldBody.Position;
        HoldRotation = HeldBody.Rotation;

        HeldBody.Sleeping = false;
        HeldBody.AutoSleep = false;
    }

    private void GrabEnd()
    {
        if ( HeldBody.IsValid() )
        {
            HeldBody.AutoSleep = true;
        }

        var grabbedObject = GrabbedObject;
        if ( grabbedObject.IsValid() )
        {
            GameManager.Instance.BroadcastRemoveTag( GrabbedObjectId, GrabbedTag );
            GameManager.Instance.BroadcastRemoveHighlight( GrabbedObjectId );
        }

        GrabbedObjectId = Guid.Empty;

        HeldBody = null;
        Grabbing = false;
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

    void GrabMove( Vector3 startPosition, Vector3 dir, Rotation rot, bool snapAngles )
    {
        if ( !HeldBody.IsValid() ) return;

        HoldPosition = startPosition - HeldPosition * HeldBody.Rotation + dir * HoldDistance;

        var grabbedObject = GrabbedObject;
        if ( grabbedObject.Tags.Has( "player" ) )
        {
            var player = grabbedObject.Root.Components.Get<Player>();
            if ( player.IsValid() )
            {
                var velocity = player.CharacterController.Velocity;
                Vector3.SmoothDamp( player.Transform.Position, HoldPosition, ref velocity, 0.075f, Time.Delta );
                player.BroadcastSetVelocity( velocity );
            }
        }

        HoldRotation = rot * HeldRotation;

        if ( snapAngles )
        {
            var angles = HoldRotation.Angles();
            HoldRotation = Rotation.From(
                MathF.Round( angles.pitch / RotateSnapAt ) * RotateSnapAt,
                MathF.Round( angles.yaw / RotateSnapAt ) * RotateSnapAt,
                MathF.Round( angles.roll / RotateSnapAt ) * RotateSnapAt
            );
        }
    }

    private void MoveTargetDistance( float distance )
    {
        HoldDistance += distance;
        HoldDistance = HoldDistance.Clamp( MinTargetDistance, MaxTargetDistance );
    }

    protected virtual void DoRotate( Rotation eye, Vector3 input )
    {
        var localRot = eye;
        localRot *= Rotation.FromAxis( Vector3.Up, input.x * RotateSpeed );
        localRot *= Rotation.FromAxis( Vector3.Right, input.y * RotateSpeed );
        localRot = eye.Inverse * localRot;

        HeldRotation = localRot * HeldRotation;
    }

    void UpdateBeam()
    {
        if ( !IsEquipped ) return;
        if ( !Player.IsValid() ) return;

        var startPos = Muzzle.Transform.Position;

        var viewModel = Player.ViewModel;
        if ( Player.IsFirstPerson && viewModel.IsValid() )
        {
            startPos = viewModel?.Muzzle?.Transform?.Position ?? startPos;
        }

        var endPos = lastBeamPosition;

        var grabbedObject = GrabbedObject;
        if ( grabbedObject.IsValid() )
        {
            var modelPhysics = grabbedObject.Components.Get<ModelPhysics>();
            var physGroup = modelPhysics?.PhysicsGroup;
            if ( !physGroup.IsValid() )
            {
                var rigidbody = grabbedObject.Components.Get<Rigidbody>();
                if ( rigidbody.IsValid() )
                {
                    physGroup = rigidbody.PhysicsBody.PhysicsGroup;
                }
            }

            if ( physGroup.IsValid() )
            {
                if ( physGroup != null && GrabbedBone >= 0 )
                {
                    var physBody = physGroup.GetBody( GrabbedBone );
                    if ( physBody != null ) endPos = physBody.Transform.PointToWorld( GrabbedPosition );
                }
                else
                {
                    endPos = grabbedObject.Transform.Position;
                }
            }
            else
            {
                endPos = grabbedObject.Transform.Position + grabbedObject.Transform.Rotation * GrabbedPosition;
            }
        }

        BeamParticles.VectorPoints[0] = startPos;
        if ( HoldDistance == 0 )
            BeamParticles.VectorPoints[1] = (startPos + endPos) / 2f;
        else
            BeamParticles.VectorPoints[1] = startPos + Player.Direction.Forward * (HoldDistance * 0.9f);
        BeamParticles.VectorPoints[2] = endPos;
    }

    [Broadcast]
    void BroadcastFreezeParticles( Vector3 position )
    {
        FreezeParticles?.Clone( position );
    }
}