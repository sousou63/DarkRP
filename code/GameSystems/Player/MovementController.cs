using Sandbox.Citizen;
using GameSystems.Player;
using GameSystems;
using System;

namespace GameSystems.Player {
	/// <summary>
	/// Taken from Walker.cs
	/// </summary>
	public sealed class MovementController : Component
	{
		[Property] public CharacterController CharacterController { get; set; }
		[Property] public Collider Collider { get; set; }
		[Property] public float WalkMoveSpeed { get; set; } = 190.0f;
		[Property] public float NoClipSpeed { get; set; } = 250.0f;
		[Property] public float RunMoveSpeed { get; set; } = 190.0f;
		[Property] public float SprintMoveSpeed { get; set; } = 320.0f;

		[Property] public CitizenAnimationHelper AnimationHelper { get; set; }

		[Sync] public bool Crouching { get; set; }
		[Sync] public Angles EyeAngles { get; set; }
		[Sync] public Vector3 WishVelocity { get; set; }

		[Sync] public bool IsNoClip { get; set; }

		[Property] public bool EyesLocked { get; set; } = false;


		private PlayerConnObject player { get; set; }

		public bool WishCrouch;
		public float EyeHeight = 64;

		protected override void OnStart()
		{
			// Get the Player connection object
			// TODO better way to do it?
			var controller = GameController.Instance;
			if (controller is null) return;
			player = controller.GetPlayerByGameObjectID(GameObject.Id);
		}
		
		void TryGetPlayer()
		{
			if (player is null)
			{
				var controller = GameController.Instance;
				if (controller is null) return;
				player = controller.GetPlayerByGameObjectID(GameObject.Id);
			}
		}

		protected override void OnUpdate()
		{
			if (!IsProxy)
			{
				MouseInput();
				Transform.Rotation = new Angles( 0, EyeAngles.yaw, 0 );
			}

			UpdateAnimation();
		}

		protected override void OnFixedUpdate()
		{
			if (IsProxy)
				return;
			NoClipInput();
			CrouchingInput();
			MovementInput();
			//NoClipInput();
		}

		private void MouseInput()
		{
			if (EyesLocked) return;
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
					// Temporary fix until PlayerConnObject and NetworkHelper have been refactored;
					TryGetPlayer();
					if (player.CheckPermission(PermissionLevel.Admin)) ToggleNoClip(!IsNoClip);
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
				if (Crouching) return WalkMoveSpeed * 0.5f;
				if (IsNoClip)
				{
					if (Input.Down("run")) return NoClipSpeed * 2.5f;
					return NoClipSpeed;
				}
				if (Input.Down("run")) return SprintMoveSpeed;
				if (Input.Down("walk")) return WalkMoveSpeed;

				return RunMoveSpeed;
			}
		}

		RealTimeSince lastGrounded;
		RealTimeSince lastUngrounded;
		RealTimeSince lastJump;

		float GetFriction()
		{
			if (CharacterController.IsOnGround) return 6.0f;

			// air friction
			return 0.2f;
		}
		private void MovementInput()
		{
			if (CharacterController is null)
				return;

			var cc = CharacterController;

			Vector3 halfGravity = Scene.PhysicsWorld.Gravity * Time.Delta * 0.5f;

			WishVelocity = Input.AnalogMove;

			if (IsNoClip)
			{
				cc.IsOnGround = false;
				if (!WishVelocity.IsNearlyZero() || Input.Down("jump") || Input.Down("duck"))
				{
					// Convert input to a movement vector using EyeAngles
					var forward = EyeAngles.ToRotation().Forward;
					var right = EyeAngles.ToRotation().Right;
					var up = EyeAngles.ToRotation().Up;

					// Invert the right vector to fix reversed left and right movement
					WishVelocity = forward * WishVelocity.x - right * WishVelocity.y + up * WishVelocity.z;

					// Add upward movement if jump is pressed
					if (Input.Down("jump"))
					{
						WishVelocity += Vector3.Up;
					}
					else if (Input.Down("duck"))
					{
						WishVelocity -= Vector3.Up;
					}

					WishVelocity = WishVelocity.ClampLength(1);
					WishVelocity *= CurrentMoveSpeed;

					// Accelerate towards the desired velocity
					cc.Velocity = cc.Velocity + (WishVelocity - cc.Velocity) * Time.Delta * 10.0f;
				}

				// Apply friction to the velocity
				float friction = 5.0f;
				cc.Velocity *= 1.0f - (friction * Time.Delta);
				cc.Velocity = cc.Velocity.ClampLength(CurrentMoveSpeed);

				cc.Transform.Position += WishVelocity * Time.Delta;
				// cc.Move();
				return;
			}

			// Normal movement logic
			if (lastGrounded < 0.2f && lastJump > 0.3f && Input.Pressed("jump"))
			{
				lastJump = 0;
				cc.Punch(Vector3.Up * 300);
			}

			if (!WishVelocity.IsNearlyZero())
			{
				WishVelocity = new Angles(0, EyeAngles.yaw, 0).ToRotation() * WishVelocity;
				WishVelocity = WishVelocity.WithZ(0);
				WishVelocity = WishVelocity.ClampLength(1);
				WishVelocity *= CurrentMoveSpeed;

				if (!cc.IsOnGround)
				{
					WishVelocity = WishVelocity.ClampLength(50);
				}
			}

			cc.ApplyFriction(GetFriction());

			if (cc.IsOnGround)
			{
				cc.Accelerate(WishVelocity);
				cc.Velocity = CharacterController.Velocity.WithZ(0);
			}
			else
			{
				cc.Velocity += halfGravity;
				cc.Accelerate(WishVelocity);
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
				lastGrounded = 0;
			}
			else
			{
				lastUngrounded = 0;
			}
		}
		float DuckHeight = (64 - 36);

		bool CanUncrouch()
		{
			if (!Crouching) return true;
			if (lastUngrounded < 0.2f) return false;

			var tr = CharacterController.TraceDirection(Vector3.Up * DuckHeight);
			return !tr.Hit; // hit nothing - we can!
		}

		public void CrouchingInput()
		{
			// Dont run if noclipping
			if (IsNoClip) return;
			WishCrouch = Input.Down("duck");

			if (WishCrouch == Crouching)
				return;

			// crouch
			if (WishCrouch)
			{
				CharacterController.Height = 36;
				Crouching = WishCrouch;

				// if we're not on the ground, slide up our bbox so when we crouch
				// the bottom shrinks, instead of the top, which will mean we can reach
				// places by crouch jumping that we couldn't.
				if (!CharacterController.IsOnGround)
				{
					CharacterController.MoveTo(Transform.Position += Vector3.Up * DuckHeight, false);
					Transform.ClearInterpolation();
					EyeHeight -= DuckHeight;
				}

				return;
			}

			// uncrouch
			if (!WishCrouch)
			{
				if (!CanUncrouch()) return;

				CharacterController.Height = 64;
				Crouching = WishCrouch;
				return;
			}


		}

		private void UpdateCamera()
		{
			var camera = Scene.GetAllComponents<CameraComponent>().Where(x => x.IsMainCamera).FirstOrDefault();
			if (camera is null) return;

			var targetEyeHeight = Crouching ? 28 : 64;
			EyeHeight = EyeHeight.LerpTo(targetEyeHeight, RealTime.Delta * 10.0f);

			var targetCameraPos = Transform.Position + new Vector3(0, 0, EyeHeight);

			// smooth view z, so when going up and down stairs or ducking, it's smooth af
			if (lastUngrounded > 0.2f)
			{
				targetCameraPos.z = camera.Transform.Position.z.LerpTo(targetCameraPos.z, RealTime.Delta * 25.0f);
			}

			camera.Transform.Position = targetCameraPos;
			camera.Transform.Rotation = EyeAngles;
			camera.FieldOfView = Preferences.FieldOfView;
		}

		protected override void OnPreRender()
		{
			UpdateBodyVisibility();

			if (IsProxy)
				return;

			UpdateCamera();
		}

		private void UpdateAnimation()
		{
			if (AnimationHelper is null) return;

			var wv = WishVelocity.Length;

			AnimationHelper.WithWishVelocity(WishVelocity);
			AnimationHelper.WithVelocity(CharacterController.Velocity);
			AnimationHelper.IsGrounded = CharacterController.IsOnGround;
			AnimationHelper.DuckLevel = Crouching ? 1.0f : 0.0f;

			AnimationHelper.MoveStyle = wv < 160f ? CitizenAnimationHelper.MoveStyles.Walk : CitizenAnimationHelper.MoveStyles.Run;

			var lookDir = EyeAngles.ToRotation().Forward * 1024;
			AnimationHelper.WithLook(lookDir, 1, 0.5f, 0.25f);
		}

		private void UpdateBodyVisibility()
		{
			if (AnimationHelper is null)
				return;

			var renderMode = ModelRenderer.ShadowRenderType.On;
			if (!IsProxy) renderMode = ModelRenderer.ShadowRenderType.ShadowsOnly;

			AnimationHelper.Target.RenderType = renderMode;

			foreach (var clothing in AnimationHelper.Target.Components.GetAll<ModelRenderer>(FindMode.InChildren))
			{
				if (!clothing.Tags.Has("clothing"))
					continue;

				clothing.RenderType = renderMode;
			}
		}

	}
}
