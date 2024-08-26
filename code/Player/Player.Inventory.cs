using Sandbox.GameResources;

namespace Sandbox.GameSystems.Player;

public partial class Player
{

	// define the default Items related to all players
	[Property, Group("Inventory")] public List<WeaponResource> DefaultItems;

	[Property, Group("Inventory")] public float InventoryVisibilityDelay { get; set; } = 3f;

	private TimeSince timeSinceLastVisible = 0;

	private bool _inputDetected;

	public bool IsInventoryVisible;

	public const int MaxSlots = 9;

	public int CurrentSelectedSlot;

	// Slots for storing weapon resources
	public WeaponResource[] InventorySlots;


	private void OnStartInventory()
	{

		// Initialize the inventory slots
		InventorySlots = new WeaponResource[MaxSlots];

		// Equip all the defaults Items
		foreach ( var weaponResource in DefaultItems )
		{
			AddItem( weaponResource );
		}

	}

	protected void OnFixedUpdateInventory()
	{
		CheckForInputs();
	}

	// Add the desired item to the inventory
	public void AddItem(WeaponResource resource)
	{
		int slotIndex = resource.Slot-1;

		if ( slotIndex >= 0 && slotIndex <= MaxSlots )
		{
			InventorySlots[slotIndex] = resource;
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
		if ( slot >= 1 && slot <= MaxSlots )
		{
			var equippedItem = InventorySlots[slot-1];
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
		_inputDetected = false;
		var wheel = -Input.MouseWheel.y;

		if ( wheel > 0 || Input.Pressed( "SlotNext" ) )
		{
			CurrentSelectedSlot++;
			SlotLogicCheck();
		}

		if ( wheel < 0 || Input.Pressed( "SlotPrev" ))
		{
			CurrentSelectedSlot--;
			SlotLogicCheck();
		}

		// Check input for slots 0 to 9
		for ( int i = 0; i < 10; i++ )
		{
			if ( Input.Pressed( $"Slot{i}" ) )
			{
				// Show inventory and play sound when a slot key is pressed
				IsInventoryVisible = true;
				CurrentSelectedSlot = i;
				PlayInventoryOpenSound();
				SlotLogicCheck();
				_inputDetected = true;
				break; // Exit loop once an input is detected
			}

		}

		// Check input for SlotNext and SlotPrev
		if ( !_inputDetected && (Input.Pressed( "SlotNext" ) || Input.Pressed( "SlotPrev" )) )
		{
			IsInventoryVisible = true;
			PlayInventoryOpenSound();
			SlotLogicCheck();
			_inputDetected = true;
		}

		// Check for mouse wheel input
		if ( !_inputDetected && wheel != 0 )
		{
			IsInventoryVisible = true;
			PlayInventoryOpenSound();
			_inputDetected = true;
		}

		// Hide inventory if the delay has passed and no input was detected
		if ( timeSinceLastVisible >= InventoryVisibilityDelay && !_inputDetected )
		{
			IsInventoryVisible = false;
			timeSinceLastVisible = 0;
		}

		// Reset the timer if an input was detected
		if ( _inputDetected )
		{
			timeSinceLastVisible = 0;
		}

	}

	private void SlotLogicCheck()
	{
		if (CurrentSelectedSlot < 1) { CurrentSelectedSlot = MaxSlots; }
		if (CurrentSelectedSlot > MaxSlots ) { CurrentSelectedSlot = 1; }
		EquipItem( CurrentSelectedSlot );
	}

	private void PlayInventoryOpenSound()

	{	
		Sound.Play( "audio/select.sound" );
	}

}
