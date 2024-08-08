using Sandbox;
using System.Diagnostics;
using System.Drawing;

public sealed class PlayerInteraction : Component
{
	[Property] public float InteractRange { get; set; } = 120f;

	[Property] public bool DrawDebugInteract { get; set; } = false;

	SceneTraceResult tr;

	protected override void OnFixedUpdate()
	{
		Interact();

		// Draw the debug information if the boolean is true
		if ( DrawDebugInteract )
		{
			Interact();
			DrawDebug();
		}
	}

	void Interact()
	{
		// Get the main camera (I think this is not optimized, but anyway, I stole it from Facepunch ^^)
		var camera = Scene.GetAllComponents<CameraComponent>().Where( x => x.IsMainCamera ).FirstOrDefault();

		// Starting position of the line (camera position)
		Vector3 start = camera.Transform.World.Position;

		// Direction of the line (the direction the camera is facing)
		Vector3 direction = camera.Transform.World.Forward;

		// Calculate the end position based on direction and Interact range
		Vector3 end = start + direction * InteractRange;

		// Line Trace
		tr = Scene.Trace.Ray( start, end ).Run();

		// Check for the "interact" Tag and do some logic assiocated to it 
		if ( tr.GameObject.Tags.Has("Interact"))
		{
			// to finish, at best I would like to draw an UI on the screen " Press [Use Input] to Interact " --> then do an action assiocated with this tag
			DrawDebug();
		}

	}

	void DrawDebug()
	{
		// Draw the debug sphere at the interaction point
		Gizmo.Draw.LineSphere( tr.EndPosition, 3, 8 );

		// Show the Trace Ray Info on the console
		Log.Info( $"Hit: {tr.GameObject} at {tr.EndPosition}" );
	}
}
