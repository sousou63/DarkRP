using Sandbox;
using Sandbox.Audio;
using Sandbox.Diagnostics;
using System.Numerics;
using System;

public sealed class Inventory : Component
{

	[Property] public float InventoryVisibilityDelay { get; set; } = 3f;

	private TimeSince timeSinceLastVisible = 0;

	bool inputDetected = false;

	public bool isInventoryVisible;

	public int slots = 6;

	protected override void OnStart()
	{

	}

	protected override void OnFixedUpdate()
	{
		CheckForInputs();
	}

	public void EquipItem()
	{
		// TODO
	}

	public void RemoveItem()
	{
		// TODO
	}

	public void Hasitem()
	{
		// TODO + change to public bool
	}


	[Broadcast]
	public void DropItem()
	{
		// TODO
	}

	private void CheckForInputs()
	{
		// Reset input detection at the beginning of each check
		inputDetected = false;
		var wheel = -Input.MouseWheel.y;

		// Check input for slots 0 to 9
		for ( int i = 0; i < 10; i++ )
		{
			if ( Input.Pressed( $"Slot{i}" ) )
			{
				// Show inventory and play sound when a slot key is pressed
				isInventoryVisible = true;
				PlayInventoryOpenSound();
				inputDetected = true;
				break; // Exit loop once an input is detected
			}
		}

		// Check input for SlotNext and SlotPrev
		if ( !inputDetected && (Input.Pressed( "SlotNext" ) || Input.Pressed( "SlotPrev" )) )
		{
			isInventoryVisible = true;
			PlayInventoryOpenSound();
			inputDetected = true;
		}

		// Check for mouse wheel input
		if ( !inputDetected && wheel != 0 )
		{
			isInventoryVisible = true;
			PlayInventoryOpenSound();
			inputDetected = true;
		}

		// Hide inventory if the delay has passed and no input was detected
		if ( timeSinceLastVisible >= InventoryVisibilityDelay && !inputDetected )
		{
			isInventoryVisible = false;
			timeSinceLastVisible = 0;
		}

		// Reset the timer if an input was detected
		if ( inputDetected )
		{
			timeSinceLastVisible = 0;
		}
	}

	private void PlayInventoryOpenSound()

	{	
		Sound.Play( "audio/select.sound" );
	}

}
