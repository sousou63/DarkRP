using System;
using Sandbox.Citizen;
using Scenebox.UI;

namespace Scenebox;

public sealed class Player : Component
{
	public static Player Local => Game.ActiveScene.GetAllComponents<Player>().FirstOrDefault( x => x.Network.IsOwner );

	[RequireComponent] public CharacterController CharacterController { get; set; }
	[RequireComponent] public Inventory Inventory { get; set; }

	[Property, Group( "References" )] public GameObject Head { get; set; }
	[Property, Group( "References" )] public GameObject Body { get; set; }
	[Property, Group( "References" )] public GameObject FirstPersonView { get; set; }
	[Property, Group( "References" )] public CitizenAnimationHelper AnimationHelper { get; set; }
	[Property, Group( "References" )] public ModelPhysics ModelPhysics { get; set; }
	[Property, Group( "References" )] public Collider PlayerBoxCollider { get; set; }
	[Property, Group( "References" )] public GameObject NametagObject { get; set; }
	[Property, Group( "References" )] public GameObject FlashlightObject { get; set; }

	[Property, Group( "Movement" )] public float GroundControl { get; set; } = 4.0f;
	[Property, Group( "Movement" )] public float AirControl { get; set; } = 0.1f;
	[Property, Group( "Movement" )] public float Speed { get; set; } = 160f;
	[Property, Group( "Movement" )] public float RunSpeed { get; set; } = 290f;
	[Property, Group( "Movement" )] public float WalkSpeed { get; set; } = 90f;
	[Property, Group( "Movement" )] public float JumpForce { get; set; } = 400f;

	public Action OnJump;

	[Sync] public float Height { get; set; } = 1f;
	public float CrouchHeight = 64f;

	public bool IsFirstPerson
	{
		get => _isFirstPerson;
		set
		{
			_isFirstPerson = value;

			ShowBodyParts( !_isFirstPerson );
			if ( _isFirstPerson ) Inventory?.CurrentWeapon?.CreateViewModel();
			else Inventory?.CurrentWeapon?.ClearViewModel();
		}
	}
	bool _isFirstPerson = true;

	[Sync] public bool IsNoclipping { get; set; } = false;
	[Sync] public bool IsCrouching { get; set; } = false;
	[Sync] public bool IsSprinting { get; set; } = false;
	[Sync] public bool IsFlashlightOn { get; set; } = false;
	[Sync] public Vector3 WishVelocity { get; set; } = Vector3.Zero;
	[Sync] public Angles Direction { get; set; } = Angles.Zero;
	[Sync] public CitizenAnimationHelper.HoldTypes CurrentHoldType { get; set; } = CitizenAnimationHelper.HoldTypes.None;

	public int Health { get; set; } = 100;

	public bool CanMoveHead = true;
	public ViewModel ViewModel => Components.Get<ViewModel>( FindMode.EverythingInSelfAndDescendants );

	protected override void OnStart()
	{
		IsFirstPerson = !IsProxy;
		NametagObject.Enabled = !Network.IsOwner;
	}

	protected override void OnUpdate()
	{
		if ( !IsProxy )
		{
			if ( Input.Pressed( "Flashlight" ) )
			{
				IsFlashlightOn = !IsFlashlightOn;
				BroadcastFlashlightSound();
			}

			if ( Input.Pressed( "Noclip" ) )
			{
				IsNoclipping = !IsNoclipping;
				if ( !IsNoclipping )
				{
					CharacterController.Velocity = WishVelocity;
				}
			}

			IsSprinting = Input.Down( "Run" );
			if ( Input.Pressed( "Jump" ) ) Jump();

			Inventory.CheckWeaponConfirm();
			if ( Inventory.CurrentWeapon.IsValid() )
			{
				Inventory.CurrentWeapon.Update();
			}
			Inventory.CheckWeaponSwap();

			var cameraComponent = Scene.GetAllComponents<ICameraOverride>().Where( x => x.IsActive )?.FirstOrDefault();
			if ( cameraComponent is not null )
				cameraComponent.UpdateCamera();
			else
				UpdateCamera();
		}

		UpdateCrouch();
		UpdateAnimations();
		RotateBody();

		CanMoveHead = true;

		Components.Get<Voice>().Volume = 30f;
		FlashlightObject.Enabled = IsFlashlightOn;
	}

	protected override void OnFixedUpdate()
	{
		CharacterController.Height = CrouchHeight * Height;

		if ( !IsProxy )
		{
			if ( Input.Pressed( "View" ) ) IsFirstPerson = !IsFirstPerson;

			if ( Inventory.CurrentWeapon.IsValid() )
			{
				Inventory.CurrentWeapon.FixedUpdate();
			}

			BuildWishVelocity();
			Move();
		}
		else
		{
			Head.Transform.Rotation = Direction;
		}
	}

	void Move()
	{
		if ( IsNoclipping )
		{
			var movement = Input.AnalogMove * Direction * 1000f;
			if ( Input.Down( "Run" ) ) movement *= 3;
			Transform.Position += movement * Time.Delta;
			WishVelocity = movement;
			CharacterController.Velocity = movement;
			return;
		}

		var gravity = Scene.PhysicsWorld.Gravity;

		if ( CharacterController.IsOnGround )
		{
			CharacterController.Velocity = CharacterController.Velocity.WithZ( 0 );
			CharacterController.Accelerate( WishVelocity );
			CharacterController.ApplyFriction( GroundControl );
		}
		else
		{
			CharacterController.Velocity += gravity * Time.Delta * 0.5f;
			CharacterController.Accelerate( WishVelocity );
			CharacterController.ApplyFriction( AirControl );
		}

		CharacterController.Move();

		if ( CharacterController.IsOnGround )
		{
			CharacterController.Velocity = CharacterController.Velocity.WithZ( 0 );
		}
		else
		{
			CharacterController.Velocity += gravity * Time.Delta * 0.5f;
		}
	}

	void Jump()
	{
		if ( !CharacterController.IsOnGround ) return;

		CharacterController.Punch( Vector3.Up * JumpForce );
		OnJump?.Invoke();
		BroadcastJumpAnimation();
	}

	void BuildWishVelocity()
	{
		Vector3 wishVelocity = 0;

		var rot = Head.Transform.Rotation;
		if ( Input.Down( "Forward" ) ) wishVelocity += rot.Forward;
		if ( Input.Down( "Backward" ) ) wishVelocity += rot.Backward;
		if ( Input.Down( "Left" ) ) wishVelocity += rot.Left;
		if ( Input.Down( "Right" ) ) wishVelocity += rot.Right;

		wishVelocity = wishVelocity.WithZ( 0 );

		if ( !wishVelocity.IsNearZeroLength ) wishVelocity = wishVelocity.Normal;

		if ( IsCrouching ) wishVelocity *= WalkSpeed;
		else if ( IsSprinting ) wishVelocity *= RunSpeed;
		else wishVelocity *= Speed;

		WishVelocity = wishVelocity;
	}

	void UpdateCamera()
	{
		var eyeAngles = Head.Transform.Rotation.Angles();
		var sens = Preferences.Sensitivity;
		if ( CanMoveHead )
		{
			eyeAngles.pitch += Input.MouseDelta.y * sens / 100f;
			eyeAngles.yaw -= Input.MouseDelta.x * sens / 100f;
		}
		eyeAngles.roll = 0f;
		eyeAngles.pitch = eyeAngles.pitch.Clamp( -89.9f, 89.9f );
		Head.Transform.Rotation = eyeAngles;

		var camPos = Head.Transform.Position;
		if ( !IsFirstPerson )
		{
			var camForward = eyeAngles.Forward;
			var camTrace = Scene.Trace.Ray( camPos, camPos - (camForward * 150) )
				.WithoutTags( "player", "trigger" )
				.Run();

			if ( camTrace.Hit )
			{
				camPos = camTrace.HitPosition + camTrace.Normal;
			}
			else
			{
				camPos = camTrace.EndPosition;
			}
		}

		Scene.Camera.Transform.Position = camPos;
		Scene.Camera.Transform.Rotation = eyeAngles;
		Scene.Camera.FieldOfView = SceneboxPreferences.Settings.FieldOfView;
		Direction = eyeAngles;
	}

	void UpdateCrouch()
	{
		if ( !IsProxy )
		{
			IsCrouching = Input.Down( "Duck" );
		}

		CrouchHeight = CrouchHeight.LerpTo( IsCrouching ? 32f : 64f, 1f - MathF.Pow( 0.5f, Time.Delta * 25f ) );
		Head.Transform.LocalPosition = Head.Transform.LocalPosition.WithZ( CrouchHeight );
	}

	void UpdateAnimations()
	{
		if ( AnimationHelper is null ) return;

		AnimationHelper.WithWishVelocity( WishVelocity );
		AnimationHelper.WithVelocity( IsProxy ? WishVelocity : CharacterController.Velocity );
		AnimationHelper.AimAngle = Direction;
		AnimationHelper.IsGrounded = CharacterController.IsOnGround;
		AnimationHelper.IsNoclipping = IsNoclipping;
		AnimationHelper.WithLook( Direction.Forward );
		AnimationHelper.MoveStyle = CitizenAnimationHelper.MoveStyles.Run;
		AnimationHelper.DuckLevel = IsCrouching ? 1f : 0f;

		AnimationHelper.HoldType = CurrentHoldType;
	}

	void RotateBody()
	{
		if ( Body is null ) return;

		var targetAngle = new Angles( 0, Direction.yaw, 0 ).ToRotation();
		float rotateDiff = Body.Transform.Rotation.Distance( targetAngle );

		if ( rotateDiff > 50f || CharacterController.Velocity.Length > 10f )
		{
			Body.Transform.Rotation = Rotation.Lerp( Body.Transform.Rotation, targetAngle, Time.Delta * 10f );
		}
		else
		{
			Body.Transform.Rotation = targetAngle;
		}
	}

	void ShowBodyParts( bool show )
	{
		var renderers = AnimationHelper.GameObject.Components.GetAll<ModelRenderer>( FindMode.EverythingInSelfAndDescendants );
		foreach ( var renderer in renderers )
		{
			renderer.RenderType = show ? ModelRenderer.ShadowRenderType.On : ModelRenderer.ShadowRenderType.ShadowsOnly;
		}
	}

	void SetRagdoll( bool enabled )
	{
		ModelPhysics.Enabled = enabled;
		AnimationHelper.Target.UseAnimGraph = !enabled;

		GameManager.Instance.BroadcastSetTag( GameObject.Id, "ragdoll", enabled );

		if ( !enabled )
		{
			GameObject.Transform.LocalPosition = Vector3.Zero;
			GameObject.Transform.LocalRotation = Rotation.Identity;
		}

		ShowBodyParts( enabled );

		Transform.ClearInterpolation();
	}

	[Broadcast]
	public void Damage( float amount, int damageType = 0 )
	{
		if ( Health <= 0 ) return;
		if ( IsProxy ) return;

		HurtOverlay.Instance?.Hurt();
		var sound = Sound.Play( "impact-melee-flesh" );
		sound.ListenLocal = true;

		Health -= (int)amount;
		if ( Health <= 0 )
		{
			Kill( damageType, Rpc.Caller.DisplayName );
		}
	}

	[Broadcast]
	public void Kill( int damageType = 0, string killer = "", bool enableRagdoll = true )
	{
		GameObject.Network.SetOwnerTransfer( OwnerTransfer.Takeover );
		GameObject.Network.SetOrphanedMode( NetworkOrphaned.Host );
		if ( enableRagdoll )
		{
			SetRagdoll( true );
			PlayerBoxCollider.Enabled = false;
			var fadeAfter = Components.GetOrCreate<FadeAfter>();
			fadeAfter.Time = 10f;
			fadeAfter.FadeTime = 4f;
		}
		else
		{
			GameObject.Tags.Set( "invisible", true );
			SetRagdoll( false );
		}
		NametagObject.Enabled = false;

		if ( IsProxy ) return;
		Health = 0;
		KillFeed.Instance?.AddEntry( killer, damageType, Network.OwnerConnection.DisplayName );

		Inventory.HolsterWeapon();
		BroadcastDestroy( GameObject.Id );
	}

	[Broadcast]
	public void BroadcastSetVelocity( Vector3 velocity )
	{
		if ( IsProxy ) return;
		CharacterController.Velocity = velocity;
	}

	[Broadcast]
	void BroadcastJumpAnimation()
	{
		AnimationHelper?.TriggerJump();
	}

	[Broadcast]
	internal void BroadcastAttackAnimation()
	{
		AnimationHelper?.Target?.Set( "b_attack", true );
	}

	[Broadcast]
	void BroadcastFlashlightSound()
	{
		var sound = Sound.Play( "flashlight.toggle" );
		if ( !IsProxy )
		{
			sound.Volume = 0.4f;
			sound.ListenLocal = true;
		}
	}

	[Broadcast]
	void BroadcastDestroy( Guid id )
	{
		var gameObject = Scene.Directory.FindByGuid( id );
		if ( gameObject.IsValid() )
		{
			AnimationHelper.Components.GetOrCreate<PropHelper>();
			Components.Get<Inventory>()?.Destroy();
			Components.Get<CharacterController>()?.Destroy();
			Components.Get<Voice>()?.Destroy();
			Components.Get<Player>()?.Destroy();
		}
	}
}
