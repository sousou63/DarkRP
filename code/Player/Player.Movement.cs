using System;
using GameSystems;
using GameSystems.Player;
using Sandbox.Citizen;

namespace Sandbox.GameSystems.Player;

/// <summary>
/// Taken from Walker.cs
/// </summary>
public partial class Player
{
	[Property, Group("Movement")] public bool EyesLocked { get; set; } = false;
	[Property, Group("Movement")] public float WalkMoveSpeed { get; set; } = 190.0f;
	[Property, Group("Movement")] public float NoClipSpeed { get; set; } = 250.0f;
	[Property, Group("Movement")] public float RunMoveSpeed { get; set; } = 190.0f;
	[Property, Group("Movement")] public float SprintMoveSpeed { get; set; } = 320.0f;
	[Property, Group("Movement")] public Collider Collider { get; set; }
	[Property, Group("Movement")] public CharacterController CharacterController { get; set; }
	[Property, Group("Movement")] public CitizenAnimationHelper AnimationHelper { get; set; }

	[Sync, HostSync] public bool IsNoClip { get; set; }
	[Sync] public Angles EyeAngles { get; set; }
	[Sync] public bool _crouching { get; set; }
	[Sync] private Vector3 _wishVelocity { get; set; }

	private bool _wishCrouch;

	private float _eyeHeight = 64;

	private RealTimeSince _lastGrounded;

	private RealTimeSince _lastUngrounded;

	private RealTimeSince _lastJump;

	public void OnStartMovement()
	{
		// Get the Player connection object
		// TODO better way to do it?
		var controller = GameController.Instance;
		if ( controller is null ) { return; }
	}

	void OnUpdateMovement()
	{
		if (!IsProxy)
		{
			MouseInput();
			Transform.Rotation = new Angles( 0, EyeAngles.yaw, 0 );
		}

		UpdateAnimation();
	}

	protected void OnFixedUpdateMovement()
	{
		if ( IsProxy ) { return; }
		NoClipInput();
		CrouchingInput();
		MovementInput();
	}

	private void MouseInput()
	{
		if ( EyesLocked ) { return; }
		var e = EyeAngles;
		e += Input.AnalogLook;
		e.pitch = e.pitch.Clamp(-90, 90);
		e.roll = 0.0f;
		EyeAngles = e;
	}
	private void NoClipInput()
	{
		if (Input.Pressed("noclip"))
		{
			try{
				if ( GameController.Instance.GetPlayerByGameObjectId( GameObject.Id )
				    .CheckPermission( PermissionLevel.Admin ) )
				{
					ToggleNoClip(!IsNoClip);
				}
			}catch (Exception e){
				Log.Warning($"Player {GameObject.Name} tried to toggle noclip but failed.");
				Log.Warning(e);
			}
		}
	}

	public void ToggleNoClip(bool enabled)
	{
		IsNoClip = enabled;
		Collider.Enabled = !IsNoClip;
	}

	float CurrentMoveSpeed
	{
		get
		{
			if ( _crouching ) { return WalkMoveSpeed * 0.5f; }
			if ( IsNoClip )	  { return NoClipSpeed * (Input.Down("run") ? 2.5f : 1f);}
			if ( Input.Down( "run" ) )  { return SprintMoveSpeed; }
			if ( Input.Down( "walk" ) ) { return WalkMoveSpeed; }

			return RunMoveSpeed;
		}
	}


	float GetFriction()
	{
		// Ground or air friction
		return CharacterController.IsOnGround ? 6.0f : 0.2f;
	}
	private void MovementInput()
	{
		var cc = CharacterController;
		if ( cc is null ) { return; }

		Vector3 halfGravity = Scene.PhysicsWorld.Gravity * Time.Delta * 0.5f;

		_wishVelocity = Input.AnalogMove;

		if (IsNoClip)
		{
			cc.IsOnGround = false;
			if (!_wishVelocity.IsNearlyZero() || Input.Down("jump") || Input.Down("duck"))
			{
				// Convert input to a movement vector using EyeAngles
				var forward = EyeAngles.ToRotation().Forward;
				var right = EyeAngles.ToRotation().Right;
				var up = EyeAngles.ToRotation().Up;

				// Invert the right vector to fix reversed left and right movement
				_wishVelocity = forward * _wishVelocity.x - right * _wishVelocity.y + up * _wishVelocity.z;

				// Add upward movement if jump is pressed
				if (Input.Down("jump"))
				{
					_wishVelocity += Vector3.Up;
				}
				else if (Input.Down("duck"))
				{
					_wishVelocity -= Vector3.Up;
				}

				_wishVelocity = _wishVelocity.ClampLength(1);
				_wishVelocity *= CurrentMoveSpeed;

				// Accelerate towards the desired velocity
				cc.Velocity += (_wishVelocity - cc.Velocity) * Time.Delta * 10.0f;
			}

			// Apply friction to the velocity
			float friction = 5.0f;
			cc.Velocity *= 1.0f - (friction * Time.Delta);
			cc.Velocity = cc.Velocity.ClampLength(CurrentMoveSpeed);

			cc.Transform.Position += _wishVelocity * Time.Delta;
			// cc.Move();
			return;
		}

		// Normal movement logic
		if (_lastGrounded < 0.2f && _lastJump > 0.3f && Input.Pressed("jump"))
		{
			_lastJump = 0;
			cc.Punch(Vector3.Up * 300);
		}

		if (!_wishVelocity.IsNearlyZero())
		{
			_wishVelocity = new Angles(0, EyeAngles.yaw, 0).ToRotation() * _wishVelocity;
			_wishVelocity = _wishVelocity.WithZ(0);
			_wishVelocity = _wishVelocity.ClampLength(1);
			_wishVelocity *= CurrentMoveSpeed;

			if (!cc.IsOnGround)
			{
				_wishVelocity = _wishVelocity.ClampLength(50);
			}
		}

		cc.ApplyFriction(GetFriction());

		if (cc.IsOnGround)
		{
			cc.Accelerate(_wishVelocity);
			cc.Velocity = CharacterController.Velocity.WithZ(0);
		}
		else
		{
			cc.Velocity += halfGravity;
			cc.Accelerate(_wishVelocity);
		}

		// Don't walk through other players, let them push you out of the way
		var pushVelocity = PlayerPusher.GetPushVector(Transform.Position + Vector3.Up * 40.0f, Scene, GameObject);
		if (!pushVelocity.IsNearlyZero())
		{
			var travelDot = cc.Velocity.Dot(pushVelocity.Normal);
			if (travelDot < 0)
			{
				cc.Velocity -= pushVelocity.Normal * travelDot * 0.6f;
			}
		
			cc.Velocity += pushVelocity * 128.0f;
		}

		cc.Move();

		if (!cc.IsOnGround)
		{
			cc.Velocity += halfGravity;
		}
		else
		{
			cc.Velocity = cc.Velocity.WithZ(0);
		}

		if (cc.IsOnGround)
		{
			_lastGrounded = 0;
		}
		else
		{
			_lastUngrounded = 0;
		}
	}
	float DuckHeight = (64 - 36);

	bool CanUncrouch()
	{
		if ( !_crouching ) { return true; }
		if ( _lastUngrounded < 0.2f ) { return false; }

		var tr = CharacterController.TraceDirection(Vector3.Up * DuckHeight);
		return !tr.Hit; // hit nothing - we can!
	}

	public void CrouchingInput()
	{
		// Dont run if noclipping
		if ( IsNoClip ) { return; }
		_wishCrouch = Input.Down("duck");

		if ( _wishCrouch == _crouching ) { return; }

		// crouch
		if (_wishCrouch)
		{
			CharacterController.Height = 36;
			_crouching = _wishCrouch;

			// if we're not on the ground, slide up our bbox so when we crouch
			// the bottom shrinks, instead of the top, which will mean we can reach
			// places by crouch jumping that we couldn't.
			if (!CharacterController.IsOnGround)
			{
				CharacterController.MoveTo(Transform.Position += Vector3.Up * DuckHeight, false);
				Transform.ClearInterpolation();
				_eyeHeight -= DuckHeight;
			}
			return;
		}

		// uncrouch
		if ( !_wishCrouch )
		{
			if ( !CanUncrouch() ) { return; }

			CharacterController.Height = 64;
			_crouching = _wishCrouch;
			return;
		}
	}

	private void UpdateCamera()
	{
		if ( _camera is null ) { return; }

		var targetEyeHeight = _crouching ? 28 : 64;
		_eyeHeight = _eyeHeight.LerpTo(targetEyeHeight, RealTime.Delta * 10.0f);

		var targetCameraPos = Transform.Position + new Vector3(0, 0, _eyeHeight);

		// smooth view z, so when going up and down stairs or ducking, it's smooth af
		if (_lastUngrounded > 0.2f)
		{
			targetCameraPos.z = _camera.Transform.Position.z.LerpTo(targetCameraPos.z, RealTime.Delta * 25.0f);
		}

		_camera.Transform.Position = targetCameraPos;
		_camera.Transform.Rotation = EyeAngles;
		_camera.FieldOfView = Preferences.FieldOfView;
	}

	protected override void OnPreRender()
	{
		UpdateBodyVisibility();
		if ( IsProxy ) { return; }

		UpdateCamera();
	}

	private void UpdateAnimation()
	{
		if ( AnimationHelper is null ) { return; }

		var wv = _wishVelocity.Length;

		AnimationHelper.WithWishVelocity(_wishVelocity);
		AnimationHelper.WithVelocity(CharacterController.Velocity);
		AnimationHelper.IsGrounded = CharacterController.IsOnGround;
		AnimationHelper.DuckLevel = _crouching ? 1.0f : 0.0f;

		AnimationHelper.MoveStyle = wv < 160f ? CitizenAnimationHelper.MoveStyles.Walk : CitizenAnimationHelper.MoveStyles.Run;

		var lookDir = EyeAngles.ToRotation().Forward * 1024;
		AnimationHelper.WithLook(lookDir, 1, 0.5f, 0.25f);
	}

	private void UpdateBodyVisibility()
	{
		if ( AnimationHelper is null ) { return; }

		var renderMode = ModelRenderer.ShadowRenderType.On;
		if ( !IsProxy )
		{ 
			renderMode = ModelRenderer.ShadowRenderType.ShadowsOnly;
		}

		AnimationHelper.Target.RenderType = renderMode;

		foreach (var clothing in AnimationHelper.Target.Components.GetAll<ModelRenderer>(FindMode.InChildren))
		{
			if ( !clothing.Tags.Has( "clothing" ) ) { continue; }

			clothing.RenderType = renderMode;
		}
	}
}
