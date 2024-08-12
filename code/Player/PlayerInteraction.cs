using Sandbox;
using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Drawing;

public sealed class PlayerInteraction : Component
{
	[Property] public float InteractRange { get; set; } = 120f;

	[Property] public bool DrawDebugInteract { get; set; } = false;

	[Property] public string InteractTag { get; set; } = "Interact"; // Interact tag to add to desired interactable objects

	SceneTraceResult tr;

	protected override void OnFixedUpdate()
	{
		// here we make sure that the interact function is only firing for the player network owner only ( maybe I can just check for the local player instead idk )
		if ( Network.IsOwner )
		{
			Interact();

			// Draw the debug information if the boolean is true
			if ( DrawDebugInteract )
			{
				DrawDebug();
			}

		}
	}

	void Interact()
	{
		// Get the main camera 
		var camera = Gizmo.CameraTransform;

		// Starting position of the line (camera position)
		Vector3 start = camera.Position;

		// Direction of the line (the direction the camera is facing)
		Vector3 direction = camera.Forward;

		// Calculate the end position based on direction and Interact range
		Vector3 end = start + direction * InteractRange;

		// Line Trace
		tr = Scene.Trace.Ray( start, end ).Run();

		// Check for the "interact" Tag and do some logic associated to it 
		if ( tr.GameObject != null && tr.GameObject.Tags.Has( InteractTag ) )
		{
			// Is there a better way to do this?
			if ( Input.Pressed( "Use" ) )
			{
				HandleInteraction( "Use" );
			}
			if ( Input.Pressed( "Use Special" ) )
			{
				HandleInteraction( "Use Special" );
			}
			if ( Input.Pressed( "attack1" ) )
			{
				HandleInteraction( "attack1" );
			}
			if ( Input.Pressed( "attack2" ) )
			{
				HandleInteraction( "attack2" );
			}
		}
		else
		{
			//Log.Warning( "Hit object is null or does not have the interact tag." );
		}
	}

	void DrawDebug()
	{

		// Draw the debug sphere at the interaction point
		Gizmo.Draw.LineSphere( tr.EndPosition, 3, 8 );

		// Show the Trace Ray Info on the console
		Log.Info( $"Hit: {tr.GameObject} at {tr.EndPosition}" );
	}

	public Vector3 ForwardLineTrace()

	{

		// Get the main camera 
		var camera = Gizmo.CameraTransform;

		// Starting position of the line (camera position)
		Vector3 start = camera.Position;

		// Direction of the line (the direction the camera is facing)
		Vector3 direction = camera.Forward;

		// Calculate the end position based on direction and Interact range
		Vector3 end = start + direction * InteractRange;

		// Line Trace
		tr = Scene.Trace.Ray( start, end ).Run();

		// Check If the Hit is valid 
		if ( (tr.GameObject != null) && tr.Hit )
		{
			// return the Hit Position
			return (tr.EndPosition);
		}

		// Return a default value if no hit was detected
		return Vector3.Zero;

	}

	private void HandleInteraction( string inputKey )
	{
		try
		{
			var interactable = tr.GameObject.Components.Get<IInteractable>();
			if ( interactable == null ) return;

			switch ( inputKey )
			{
				case "Use":
					interactable.InteractUse( tr, GameObject );
					break;
				case "Use Special":
					interactable.InteractSpecial( tr, GameObject );
					break;
				case "attack1":
					interactable.InteractAttack1( tr, GameObject );
					break;
				case "attack2":
					interactable.InteractAttack2( tr, GameObject );
					break;
				default:
					break;
			}
		}
		catch ( Exception e )
		{
			Log.Error( e );
		}
	}
}
