using GameSystems.Player;

namespace Sandbox.Weapons.Default;

// Reference: https://github.com/CarsonKompon/sbox-scenebox-2/blob/main/code/Weapons/Physgun.cs
// TODO: Tie into inventory https://github.com/CarsonKompon/sbox-scenebox-2
// TODO: Precision mode (For props) - This is to replicate Physgun, let's you adjust hold distance, snapping, etc.
// TODO: Add server tag to prevent people from stealing your held items
public class Hands : Weapon
{
	[Property] public string GrabbableTag { get; set; } = "grab";

    [Property] private float InteractRange { get; set; } = 150f;
    [Property] private float ThrowForce { get; set; } = 450f;
    [Property] private float MaxReleaseVelocity { get; set; } = 500f;
    [Property] private float RotateSpeed { get; set; } = 1f;

    [Property] private float HoldDistance { get; set; } = 70f;
    private float _heldDistance;
    private Rotation _heldRotation = Rotation.Identity;

    // References
    [Property] private MovementController MovementController { get; set; }
    private CameraComponent _camera;
	
	private GameObject _held;
	private PhysicsBody _heldBody;
	private Vector3 _heldCenter;
	
	private float _lastPickupTime;
	private const float DeltaPickupTime = 0.5f;
	
	public bool IsHolding() => _held != null;

	protected override void OnStart()
	{
		if ( IsProxy )
		{
			Enabled = false;
			return;
		}
		
		_camera = Scene.Camera;
	}
	
	protected override void OnUpdate()
	{
		if ( IsHolding() )
		{
			// Rotate the object if the player is holding down the rotate button
			if ( Input.Down( "attack3" ) ) {
				RotateHeldObject();
			} else if (Input.Released("attack3")) {
				UnlockHeldObject();
			} else if ( Input.Down( "reload" ) ) {
				ResetRotationHeldObject();
			} else if (Input.Down("attack2")) {
				Release(ThrowForce);
			} else if (Input.Released("attack1") && RealTime.Now - _lastPickupTime > DeltaPickupTime  ) {
				Release();
			}
		} else if ( Input.Down( "attack1" ) )
		{
			AttemptGrab();
		}
	}
	
	protected override void OnFixedUpdate()
	{
		if ( _held == null )
		{
			return;
		}

		if ( !_held.IsValid )
		{
			Release();
			return;
		}
		
		var holdPosition = _camera.Transform.Position + _camera.Transform.World.Forward * HoldDistance;

		// Check if the object is too far away from the hold position
		var heldDistance = Vector3.DistanceBetween(_held.Transform.Position, holdPosition);
		if (heldDistance > InteractRange) { 
			Release(); 
			return;
		}

		var velocity = _heldBody.Velocity;
		Vector3.SmoothDamp( _heldBody.Position, holdPosition, ref velocity, 0.075f, Time.Delta );
		_heldBody.Velocity = velocity;

		var angularVelocity = _heldBody.AngularVelocity;
		Rotation.SmoothDamp( _heldBody.Rotation, _heldRotation, ref angularVelocity, 0.075f, Time.Delta );
		_heldBody.AngularVelocity = angularVelocity;
	}
	
	private void AttemptGrab()
	{
		// Starting position of the line (camera position)
		var start = _camera.Transform.Position;

		// Direction of the line (the direction the camera is facing)
		var direction = _camera.Transform.World.Forward;

		// Calculate the end position based on direction and interact range
		var end = start + direction * InteractRange;

		// Perform a line trace (raycast) to detect objects in the line of sight ( raycast ignore the player )
		var tr = Scene.Trace.Ray( start, end )
			.UseHitboxes()
			.IgnoreGameObject( GameObject )
			.WithTag( GrabbableTag )
			.Run();
		
		if ( !tr.Hit || !tr.GameObject.IsValid() || tr.GameObject.Tags.Has( "map" ) || tr.StartedSolid ) return;
		
		var rootObject = tr.GameObject.Root;
		var body = tr.Body;
		
		if ( !body.IsValid() )
		{
			if ( rootObject.IsValid() )
			{
				var rb = rootObject.Components.Get<Rigidbody>( FindMode.EverythingInSelfAndDescendants );
				if ( rb.IsValid() )
				{
					body = rb.PhysicsBody;
				}
			}
		}

		if ( !body.IsValid() ) return;
		
		// Don't move keyframed
		if ( body.BodyType == PhysicsBodyType.Keyframed ) return;

		Grab(tr.GameObject, tr.Body);
	}

	public void Grab( GameObject target, PhysicsBody targetBody )
	{
		target.Network.TakeOwnership();
		
		_heldDistance = HoldDistance;
		_heldRotation = target.Transform.Rotation;
		
		_held = target;
		_heldBody = targetBody;
		
		// Get the bounds of the object
		var bounds = target.GetBounds();
		_heldCenter = bounds.Center;

		_lastPickupTime = RealTime.Now;
	}

	public void Release(float throwingForce = 0)
	{
		UnlockHeldObject();
		
		if ( _heldBody.IsValid() )
		{
			_heldBody.AutoSleep = true;
			
			// Cap the velocity
			var currentVelocity = _heldBody.Velocity;
			if (currentVelocity.Length > MaxReleaseVelocity)
			{
				currentVelocity = currentVelocity.Normal * MaxReleaseVelocity;
				_heldBody.Velocity = currentVelocity;
			}
			
			_heldBody.ApplyImpulse(_camera.Transform.World.Forward * _heldBody.Mass * throwingForce);
		}
		
		_held = null;
		_heldBody = null;
	}

	private void RotateHeldObject()
	{
		MovementController.EyesLocked = true;

		var input = Input.MouseDelta * RotateSpeed;
    
		var eyeRot = MovementController.EyeAngles.ToRotation();
    
		// Create rotation around local X and Y axes
		var rotX = Rotation.FromAxis(eyeRot * Vector3.Right, input.y);
		var rotY = Rotation.FromAxis(eyeRot * Vector3.Up, input.x);
    
		// Combine rotations
		var newRot = rotY * rotX;
    
		// Apply to current held rotation
		_heldRotation = newRot * _heldRotation;
	}

	private void ResetRotationHeldObject()
	{
		_heldRotation = Rotation.Identity;
	}

	private void UnlockHeldObject()
	{
		MovementController.EyesLocked = false;
	}
}
