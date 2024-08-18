using System;
using Entity.Interactable;

namespace GameSystems.Interaction
{

	public sealed class InteractionSystem : Component
	{
		// Interaction range 
		[Property] public float InteractRange { get; set; } = 120f;

		// Toggle for drawing debug information related to interactions
		[Property] public bool DrawDebugInteract { get; set; } = false;

		public CameraComponent CameraComponent { get; set; }

		// Tag that marks objects as interactable
		[Property] public string InteractTag { get; set; } = "Interact";

		[Property, Sync] 
        public GameObject HoldingArea { get; set; }

		private PickupSystem pickupSystem;

		private SceneTraceResult tr;

		protected override void OnAwake()
		{
			CameraComponent = Scene.Camera;
			 
			pickupSystem = new PickupSystem(
				InteractRange,
				GameObject.Components.Get<Player.MovementController>(),
				HoldingArea
				);
		}

		protected override void OnFixedUpdate()
		{
			// Ensure that interaction logic only runs for the player who owns the network object
			if (Network.IsOwner)
			{
				// Execute the interaction logic
				Interact();

				// Draw the debug information if the option is enabled
				if (DrawDebugInteract)
				{
					DrawDebug();
				}
				pickupSystem.UpdateHeldObject();
			}
		}

		/// <summary>
		/// Handles the interaction logic by tracing a ray from the camera and checking if it hits an interactable object.
		/// </summary>
		void Interact()
		{
			// Get the main camera transform
			var camera = CameraComponent.Transform;

			// Starting position of the line (camera position)
			Vector3 start = camera.Position;

			// Direction of the line (the direction the camera is facing)
			Vector3 direction = camera.World.Forward;

			// Calculate the end position based on direction and interact range
			Vector3 end = start + direction * InteractRange;

			// Perform a line trace (raycast) to detect objects in the line of sight ( raycast ignore the player )
			tr = Scene.Trace.IgnoreGameObject(GameObject).Ray( start, end ).Run();

			// Check if the hit object has the "interact" tag and handle the interaction
			if (tr.GameObject != null && tr.GameObject.Tags.Has(InteractTag))
			{
				// Handle the different interaction types based on input
				// Is there a better way to do this?
				if (Input.Pressed("Use"))
				{
					HandleInteraction("Use");
				}
				if (Input.Pressed("Use Special"))
				{
					HandleInteraction("Use Special");
				}
				if (Input.Pressed("attack1"))
				{
					HandleInteraction("attack1");
				}
				if (Input.Pressed("attack2"))
				{
					HandleInteraction("attack2");
				}
			}

			// Optional: Handle cases where no valid object is hit
			// Log.Warning( "Hit object is null or does not have the interact tag." );
		}

		/// <summary>
		/// Draws debug information, such as the hit point of the interaction ray.
		/// </summary>
		void DrawDebug()
		{
			// Draw a debug sphere at the interaction point
			Gizmo.Draw.LineSphere(tr.EndPosition, 3, 8);

			// Log the trace ray information to the console
			Log.Info($"Hit: {tr.GameObject} at {tr.EndPosition}");
		}

		/// <summary>
		/// Performs a forward line trace from the camera and returns the hit position.
		/// </summary>
		/// <returns>The position of the hit point, or Vector3.Zero if no hit is detected.</returns>
		public Vector3 ForwardLineTrace()
		{
			try
			{
				// Get the main camera transform
				var camera = CameraComponent.Transform;

				// Starting position of the line (camera position)
				Vector3 start = camera.Position;

				// Direction of the line (the direction the camera is facing)
				Vector3 direction = camera.World.Forward;

				// Calculate the end position based on direction and interact range
				Vector3 end = start + direction * InteractRange;

				// Perform a line trace (raycast) to detect objects in the line of sight ( raycast ignore the player )
				tr = Scene.Trace.IgnoreGameObject( GameObject ).Ray( start, end ).Run();

				// Check if the trace hit is valid
				if ((tr.GameObject != null) && tr.Hit)
				{
					// Return the hit position
					return tr.EndPosition;
				}

				// Return a default value if no hit is detected
				return Vector3.Zero;
			}
			catch (Exception e)
			{
				// Log any errors that occur during the line trace
				Log.Error(e);
				return Vector3.Zero;
			}
		}


		public void TryPickup(GameObject gameObject)
		{
			if (!pickupSystem.IsHoldingObject())
				pickupSystem.HandlePickup( gameObject );
		}
		/// <summary>
		/// Handles the interaction logic based on the provided input key.
		/// </summary>
		/// <param name="inputKey">The input key corresponding to the interaction type.</param>
		private void HandleInteraction(string inputKey)
		{
			try
			{
				// Get the interactable component from the hit object
				var interactable = tr.GameObject.Components.Get<IInteractable>();
				if (interactable == null) return;

				// Execute the appropriate interaction based on the input key
				switch (inputKey)
				{
					case "Use":
						interactable.InteractUse(tr, GameObject);
						break;
					case "Use Special":
						interactable.InteractSpecial(tr, GameObject);
						break;
					case "attack1":
						interactable.InteractAttack1(tr, GameObject);
						break;
					case "attack2":
						interactable.InteractAttack2(tr, GameObject);
						break;
					default:
						break;
				}
			}
			catch (Exception e)
			{
				// Log any errors that occur during the interaction handling
				Log.Error(e);
			}
		}
	}

}
