using System;
using Commands;
using Sandbox;

public sealed class DoorLogic : Component, IInteractable, Component.INetworkListener
{
	[Property]
	public GameObject Door { get; set; }
	[Property]
	public bool IsUnlocked { get; set; } = true;
	[Property]
	public bool IsOpen { get; set; } = false;
	[Property]
	public GameObject Owner { get; set; } = null;
	[Property]
	public int Price { get; set; } = 100;


	private Chat chat { get; set; }

	protected override void OnStart()
	{
		chat = Scene.Directory.FindComponentByGuid( new Guid( "8123a00c-4e46-4e56-bdd8-050cd2785186" ) ) as Chat;
		if ( chat == null ) Log.Error( "Chat component not found" );
	}

	public void InteractUse( SceneTraceResult tr, GameObject player )
	{
		Log.Info( "Interacting with door" );
		// Dont interact with the door if it is locked
		if ( IsUnlocked == false ) return;

		// Open / Close door
		OpenCloseDoor();
	}

	public void InteractSpecial( SceneTraceResult tr, GameObject player )
	{
		if ( Owner == null )
		{
			Log.Info( "Purchasing" );
			PurchaseDoor( player );
		}
		else
		{
			if ( Owner.Id == player.Id )
			{
				SellDoor();
			}
		}
	}

	public void InteractAttack1( SceneTraceResult tr, GameObject player )
	{
		// TODO The user should have a "keys" weapon select to do the following interactions to avoid input conflicts
		if ( player.Id == Owner?.Id )
		{
			LockDoor();
		}
		else
		{
			// TODO: Knock on the door
		}
	}

	public void InteractAttack2( SceneTraceResult tr, GameObject player )
	{
		// TODO The user should have a "keys" weapon select to do the following interactions to avoid input conflicts
		if ( player.Id == Owner?.Id )
		{
			UnlockDoor();
		}
		else
		{
			// TODO: Knock on the door
		}
	}

	private void PurchaseDoor( GameObject player )
	{
		// Get player stats
		var playerStats = player.Components.Get<PlayerStats>();
		if ( playerStats == null ) return;

		// Check if they can afford the door
		if ( playerStats.MoneyBase < Price )
		{
			chat?.NewSystemMessage( "You can't afford this door", true );
			return;
		}

		// Deduct the money
		playerStats.RemoveMoney( Price );
		Owner = player;
		chat?.NewSystemMessage( "Door has been purchased for $" + Price.ToString(), true );
	}

	private void SellDoor()
	{
		Owner = null;
		chat?.NewSystemMessage( "Door has been sold.", true );
	}

	private void OpenCloseDoor()
	{
		if ( Door == null ) return;
		IsOpen = !IsOpen;
		Door.Transform.Rotation = Door.Transform.Rotation == Rotation.From( 0, 90, 0 ) ? Rotation.From( 0, 0, 0 ) : Rotation.From( 0, 90, 0 );
	}

	private void LockDoor()
	{
		IsUnlocked = false;
		chat?.NewSystemMessage( "Door has been locked.", true );
	}

	private void UnlockDoor()
	{
		IsUnlocked = true;
		chat?.NewSystemMessage( "Door has been unlocked.", true );
	}

	// TODO this should be moved to a game controller and sells all doors owned by them. Doors should not listen to this event individually.
	void Component.INetworkListener.OnDisconnected( Connection channel )
	{
		if ( IsProxy ) return;
		if ( Owner == null ) return;
		SellDoor();
	}
}
