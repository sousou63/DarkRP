using System;
using Entity.Interactable;

namespace Sandbox.GameSystems.Player;

public partial class Player
{
		// Interaction range 
		[Property, Group("Interaction")] public float InteractRange { get; set; } = 120f;

		// Toggle for drawing debug information related to interactions
		[Property, Group("Interaction")] public bool DrawDebugInteract { get; set; } = false;
		
		// Tag that marks objects as interactable
		[Property, Group("Interaction")] public string InteractTag { get; set; } = "Interact";
        
		private SceneTraceResult _interactionTraceResult;
		
		protected void OnFixedUpdateInteraction()
		{
			// Ensure that interaction logic only runs for the player who owns the network object
			if (Network.IsProxy) return;
		
			// Execute the interaction logic
			Interact();

			// Draw the debug information if the option is enabled
			if (DrawDebugInteract)
			{
				DrawDebug();
			}
		}

		/// <summary>
		/// Handles the interaction logic by tracing a ray from the camera and checking if it hits an interactable object.
		/// </summary>
		void Interact()
		{
			// Starting position of the line (camera position)
			var start = _camera.Transform.Position;

			// Direction of the line (the direction the camera is facing)
			var direction = _camera.Transform.World.Forward;

			// Calculate the end position based on direction and interact range
			var end = start + direction * InteractRange;

			// Perform a line trace (raycast) to detect objects in the line of sight ( raycast ignore the player )
			_interactionTraceResult = Scene.Trace.IgnoreGameObject(GameObject).Ray( start, end ).Run();

			// Check if the hit object has the "interact" tag and handle the interaction
			if (_interactionTraceResult.GameObject != null && _interactionTraceResult.GameObject.Tags.Has(InteractTag))
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
			Gizmo.Draw.LineSphere(_interactionTraceResult.EndPosition, 3, 8);

			// Log the trace ray information to the console
			Log.Info($"Hit: {_interactionTraceResult.GameObject} at {_interactionTraceResult.EndPosition}");
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
				var interactable = _interactionTraceResult.GameObject.Components.Get<IInteractable>();
				if (interactable == null) return;

				// Execute the appropriate interaction based on the input key
				switch (inputKey)
				{
					case "Use":
						interactable.InteractUse(_interactionTraceResult, GameObject);
						break;
					case "Use Special":
						interactable.InteractSpecial(_interactionTraceResult, GameObject);
						break;
					case "attack1":
						interactable.InteractAttack1(_interactionTraceResult, GameObject);
						break;
					case "attack2":
						interactable.InteractAttack2(_interactionTraceResult, GameObject);
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
