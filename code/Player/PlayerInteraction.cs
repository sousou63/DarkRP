using Sandbox;
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

		Interact(); 

		// Draw the debug information if the boolean is true
		if ( DrawDebugInteract )
		{
			DrawDebug();
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

		// Check for the "interact" Tag and do some logic assiocated to it 
		if ( (tr.GameObject != null) && tr.GameObject.Tags.Has( InteractTag ) )
		{
			// Check for the printer tag + the "USE" input pressed + the printer money > 0
			if ( (tr.GameObject != null) && (tr.GameObject.Tags.Has( "Printer" )) && (Input.Pressed( "Use" )) && (tr.GameObject.Components.Get<PrinterLogic>().PrinterCurrentMoney > 0) )
			{
				// Add the printer money to the player money then set the printer money to 0 ( Very early, need a verification bool in the future )
				GameObject.Components.Get<PlayerStats>().AddMoney( tr.GameObject.Components.Get<PrinterLogic>().PrinterCurrentMoney );
				tr.GameObject.Components.Get<PrinterLogic>().ResetPrinterMoney();
				Sound.Play( "audio/money.sound" );
			}

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
}
