using Sandbox;
using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Drawing;
using Sandbox.Citizen;
using System.Linq;

public sealed class PlayerInteraction : Component
{
	[Property] public float InteractRange { get; set; } = 120f;

	[Property] public bool DrawDebugInteract { get; set; } = false;

	[Property] public string InteractTag { get; set; } = "Interact"; // Interact tag to add to desired interactable objects


	/// <summary>
	/// Properties for physics holding props
	/// </summary>
	[Property, Sync] public GameObject holdingArea { get; set; }
	private GameObject m_heldObject;
	private Rigidbody m_heldObjectRigidbody;
	private PlayerController m_playerController;
	[Property] public float pickupForce { get; set; } = 150f;
	[Property] public float throwForce { get; set; } = 500f;
	[Property] public float throwTorque { get; set; } = 500f;

	SceneTraceResult tr;



	protected override void OnAwake()
	{
		// parent the holding area to the players camera
		m_playerController = GameObject.Components.Get<PlayerController>();
	}

	protected override void OnFixedUpdate()
	{
		// here we make sure that the interact function is only firing for the player network owner only ( maybe I can just check for the local player instead idk )
		if ( Network.IsOwner )
		{
			Interact();
			SetHoldingArea();

			// Draw the debug information if the boolean is true
			if ( DrawDebugInteract )
			{
				DrawDebug();
			}
			MoveHeldObject();
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
		if ( tr.GameObject != null  )
		{
			if (tr.GameObject.Tags.Has( InteractTag ) )
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
			}
			if ( Input.Pressed( "attack1" ) )
			{
				HandleInteraction( "attack1" );
			}
			if ( Input.Pressed( "attack2" ) )
			{
				if ( tr.GameObject.Tags.Has("prop") && m_heldObject == null) {
					HandlePickup(tr.GameObject);
					return;
				}
				HandleInteraction( "attack2" );
			}
		}
		else
		{

			//Log.Warning( "Hit object is null or does not have the interact tag." );
		}
		if (Input.Pressed("attack2") && m_heldObject != null) {
			DropPickup();
			return;
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

	private void SetHoldingArea() {
		var eyePos = m_playerController.Transform.Position + new Vector3( 0, 0, m_playerController.EyeHeight );
		var eyeAngles = m_playerController.EyeAngles.Forward;
		// rotate the target target vector according to the camera rotation from the eye position
		var targetVec = eyePos + eyeAngles * (InteractRange/2);

		holdingArea.Transform.Position = holdingArea.Transform.Position.LerpTo(targetVec,0.05f);
	}
	private void HandlePickup(GameObject pickedUpObject)
	{
		Rigidbody rb = pickedUpObject.Components.Get<Rigidbody>();
		if(rb != null)
		{
			// remove the earlier parent of the pickedUpObject 
			// i.e if someone else is holding it, remove it from their hands
			if(pickedUpObject.Parent != null)
			{
				pickedUpObject.SetParent(null);
				rb.GameObject.SetParent(null);
			}
			// maybe some other logic is preferred. Maybe not being able to steal items from others

			m_heldObjectRigidbody = rb;
			m_heldObjectRigidbody.Gravity = false;
			m_heldObjectRigidbody.ClearForces();
			m_heldObjectRigidbody.PhysicsBody.LinearDrag = 100f;

			Log.Info("Picking up object");
			//heldObjectRigidbody.PhysicsBody.Enabled = false;
			m_heldObjectRigidbody.GameObject.SetParent(holdingArea);
			m_heldObject = pickedUpObject;
		}
	}
	private void DropPickup()
	{
			Log.Info("Dropping object");
			m_heldObjectRigidbody.Gravity = true;
			//m_heldObjectRigidbody.ClearForces();
			m_heldObjectRigidbody.PhysicsBody.LinearDrag = 1f;

			m_heldObjectRigidbody.GameObject.SetParent(null);
			m_heldObject = null;
	}
	private void MoveHeldObject()
	{
		if(m_heldObject != null)
		{
			SetHoldingArea();
			float dist = Vector3.DistanceBetween(m_heldObject.Transform.Position, holdingArea.Transform.Position );
			if(dist > 1f)
			{
				Log.Info("Moving object");
				m_heldObjectRigidbody.Transform.LerpTo(holdingArea.Transform.World, 0.05f);
			}
			// Could be extended with rotating an item
			if(dist > InteractRange)
			{
				Log.Info("Throwing object");
				DropPickup();
			}
		}
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
