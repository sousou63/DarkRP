using System;

public sealed class DoorLogic : Component, IInteractable, Component.INetworkListener
{
	[Property]
	public GameObject Door { get; set; }
	[Property, Sync]
	public bool IsUnlocked { get; set; } = true;
	[Property, Sync]
	public bool IsOpen { get; set; } = false;
	[Property, Sync]
	public GameObject Owner { get; set; } = null;
	public PlayerStats OwnerStats { get; set; }
	[Property, Sync]
	public int Price { get; set; } = 100;

	public void InteractUse( SceneTraceResult tr, GameObject player )
	{
		// Dont interact with the door if it is locked
		if ( IsUnlocked == false ) return;

		// Open / Close door
		OpenCloseDoor();
	}

	public void InteractSpecial( SceneTraceResult tr, GameObject player )
	{
		if ( Owner == null )
		{
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

	[Broadcast]
	public void PurchaseDoor( GameObject player )
	{
		// Get player stats
		var playerStats = player.Components.Get<PlayerStats>();
		if ( playerStats == null ) return;

		if ( playerStats.PurchaseDoor( Price, GameObject))
		{
			// Deduct the money
			playerStats.RemoveMoney( Price );
			Owner = player;
			OwnerStats = playerStats;
		}
	}

	[Broadcast]
	public void SellDoor()
	{
		if ( Owner == null ) return;
		// Get player stats
		var playerStats = Owner.Components.Get<PlayerStats>();
		if ( playerStats == null ) return;

		if (playerStats.SellDoor( GameObject ))
		{
			Owner = null;
			OwnerStats = null;
		}
	}

	[Broadcast]
	private void OpenCloseDoor()
	{
			if (Door == null) return;
			IsOpen = !IsOpen;
	
			var currentRotation = Door.Transform.Rotation;
			var rotationIncrement = Rotation.From(0, 90, 0);
	
			Door.Transform.Rotation = IsOpen ? currentRotation * rotationIncrement : currentRotation * rotationIncrement.Inverse;
	}

	[Broadcast]
	private void LockDoor()
	{
		IsUnlocked = false;
		OwnerStats?.SendMessage( "Door has been locked." );
	}

	[Broadcast]
	private void UnlockDoor()
	{
		IsUnlocked = true;
		OwnerStats?.SendMessage( "Door has been unlocked." );
	}
}
