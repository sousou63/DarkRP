using Sandbox;
using Sandbox.Audio;
using Sandbox.Diagnostics;
using System.Numerics;
using System;
using Sandbox.GameResources;

public sealed class Inventory : Component
{

	// define the default Items related to all players
	[Property] public List<WeaponResource> DefaultItems;

	[Property] public float InventoryVisibilityDelay { get; set; } = 3f;

	private TimeSince timeSinceLastVisible = 0;

	bool inputDetected = false;

	public bool isInventoryVisible;

	public int maxSlots = 9;

	public int currentSelectedSlot;

	// Slots for storing weapon resources
	public WeaponResource[] inventorySlots;


	protected override void OnStart()
	{

		// Initialize the inventory slots
		inventorySlots = new WeaponResource[maxSlots];

		// Equip all the defaults Items
		foreach ( var weaponResource in DefaultItems )
		{
			AddItem( weaponResource );
		}

	}

	protected override void OnFixedUpdate()
	{
		CheckForInputs();
	}

	// Add the desired item to the inventory
	public void AddItem(WeaponResource resource)
	{
		int slotIndex = resource.Slot-1;

		if ( slotIndex >= 0 && slotIndex <= maxSlots )
		{
			inventorySlots[slotIndex] = resource;
			Log.Info( $"Weapon {resource.Name} equipped in slot {slotIndex + 1}" );
		}
		else
		{
			Log.Warning( "Invalid slot selected!" );
		}
	}

	// Equip the desired Item from the slot
	public void EquipItem(int slot)
	{
			if ( slot >= 1 && slot <= maxSlots )
			{
				var equippedItem = inventorySlots[slot-1];
				if ( equippedItem != null )
				{
					Log.Info( $"Equipped weapon: {equippedItem.Name} from slot {slot}" );
				}
				else
				{
					Log.Info( $"No weapon to equip in slot {slot}" );
				}
			}
			else
			{
				Log.Warning( "Invalid slot selected!" );
			}
	}

	// Remove the desired item from the inventory
	public void RemoveItem()
	{
		
	}

	// Check if the inventory have a specific item
	public void Hasitem()
	{
		// TODO + change to public bool
	}


	// Drop the item from the inventory
	[Broadcast] public void DropItem()
	{
		// TODO
	}

	private void CheckForInputs()
	{
		// Reset input detection at the beginning of each check
		inputDetected = false;
		var wheel = -Input.MouseWheel.y;

		if ( wheel > 0 || Input.Pressed( "SlotNext" ) )
		{
			currentSelectedSlot++;
			SlotLogicCheck();
		}

		if ( wheel < 0 || Input.Pressed( "SlotPrev" ))
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
				SlotLogicCheck();
				inputDetected = true;
				break; // Exit loop once an input is detected
			}

		}

		// Check input for SlotNext and SlotPrev
		if ( !inputDetected && (Input.Pressed( "SlotNext" ) || Input.Pressed( "SlotPrev" )) )
		{
			isInventoryVisible = true;
			PlayInventoryOpenSound();
			SlotLogicCheck();
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
		EquipItem( currentSelectedSlot );
	}

	private void PlayInventoryOpenSound()

	{	
		Sound.Play( "audio/select.sound" );
	}

}
