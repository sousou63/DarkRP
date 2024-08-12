using Commands;
using Sandbox;

public sealed class DoorLogic : Component, IInteractable
{
	[Property]
	public GameObject Door { get; set; }
	[Property]
	public bool IsUnlocked { get; set; } = true;
	[Property]
	public GameObject Owner { get; set; } = null;
	[Property]
	public int Price { get; set; } = 100;

	protected override void OnStart()
	{

	}

	public void Interact( SceneTraceResult tr, GameObject player )
	{
		Log.Info("Interacting with door");
		// Dont interact with the door if it is locked
		if ( IsUnlocked == false) return;

		// Open / Close door
		OpenCloseDoor();
	}

	private void PurchaseDoor( GameObject player )
	{
		// Get player stats
		var playerStats = player.Components.Get<PlayerStats>();
		if ( playerStats == null) return;

		// Check if they can afford the door
		if ( playerStats.MoneyBase < Price ) return;

		// Deduct the money
		playerStats.RemoveMoney( Price );
		Owner = player;
	}

	private void SellDoor()
	{
		Owner = null;
	}

	private void OpenCloseDoor()
	{
		Log.Info("Opening / Closing door");
		if ( Door == null ) return;
		Door.Transform.Rotation = Door.Transform.Rotation == Rotation.From( 0, 90, 0 ) ? Rotation.From( 0, 0, 0 ) : Rotation.From( 0, 90, 0 );
	}

	/// TODO
	/// player leave, sell door
}
