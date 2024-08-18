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

	public int maxSlots = 9;

	public int currentSelectedSlot;

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

		if ( wheel < 0 )
		{
			currentSelectedSlot++;
			SlotLogicCheck();
		}

		if ( wheel > 0 )
		{
			currentSelectedSlot--;
			SlotLogicCheck();
		}

		// Check input for slots 0 to 9
		for ( int i = 0; i < 10; i++ )
		{
			if ( Input.Pressed( $"Slot{i}" ) )
			{
				// Show inventory and play sound when a slot key is pressed
				isInventoryVisible = true;
				currentSelectedSlot = i;
				PlayInventoryOpenSound();
				inputDetected = true;
				break; // Exit loop once an input is detected
			}

		}

		if ( Input.Pressed( "SlotNext" ))
		{
			currentSelectedSlot++;
			SlotLogicCheck();
		}

		if ( Input.Pressed( "SlotPrev" ))
		{
			currentSelectedSlot--;
			SlotLogicCheck();
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

	private void SlotLogicCheck()
	{
		if (currentSelectedSlot < 1) { currentSelectedSlot = maxSlots; }
		if (currentSelectedSlot > maxSlots ) { currentSelectedSlot = 1; }
	}

	private void PlayInventoryOpenSound()

	{	
		Sound.Play( "audio/select.sound" );
	}

}
