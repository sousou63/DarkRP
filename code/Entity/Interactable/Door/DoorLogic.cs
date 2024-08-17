using System;
using GameSystems.Player;

namespace Entity.Interactable.Door
{
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
		public Stats OwnerStats { get; set; }
		[Property, Sync]
		public int Price { get; set; } = 100;

		public void InteractUse( SceneTraceResult tr, GameObject player )
		{
			// Dont interact with the door if it is locked
			if ( IsUnlocked == false ) return;

			// Open / Close door
			OpenCloseDoor();
		}
		public void KnockOnDoor()
		{
			Sound.Play( "audio/knock.sound", Door.Transform.World.Position );
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
			if (player.Id == Owner?.Id ) { LockDoor(); } else { KnockOnDoor(); }
		}

		public void InteractAttack2( SceneTraceResult tr, GameObject player )
		{
			// TODO The user should have a "keys" weapon select to do the following interactions to avoid input conflicts
			if (player.Id == Owner?.Id) { UnlockDoor(); } else { KnockOnDoor(); }
		}

		public void PurchaseDoor( GameObject player )
		{
			Log.Info( $"{player.Name} is purchasing the door." );
			// Get player stats
			var playerStats = player.Components.Get<Stats>();
			if ( playerStats == null ) return;

			if ( playerStats.PurchaseDoor( Price, GameObject ) )
			{
				// Deduct the money
				playerStats.RemoveMoney( Price );
				// Update the door owner
				UpdateDoorOwner( player, playerStats );
				Sound.Play( "audio/notification.sound" );
			}
		}

		[Broadcast]
		public void UpdateDoorOwner( GameObject player = null, Stats playerStats = null )
		{
			Owner = player;
			OwnerStats = playerStats;
		}

		public void SellDoor()
		{
			if ( Owner == null ) return;
			// Get player stats
			var playerStats = Owner.Components.Get<Stats>();
			if ( playerStats == null ) return;

			if ( playerStats.SellDoor( GameObject ) )
			{
				playerStats.AddMoney( Price / 2 );
				UnlockDoor();
				UpdateDoorOwner();
				Sound.Play( "audio/notification.sound" );
			}
		}

		[Broadcast]
		private void OpenCloseDoor()
		{
			if ( Door == null ) return;
			IsOpen = !IsOpen;

			var currentRotation = Door.Transform.Rotation;
			var rotationIncrement = Rotation.From( 0, 90, 0 );

			Door.Transform.Rotation = IsOpen ? currentRotation * rotationIncrement : currentRotation * rotationIncrement.Inverse;
			Sound.Play( "audio/door.sound", Door.Transform.World.Position );
		}

		[Broadcast]
		private void LockDoor()
		{
			IsUnlocked = false;
			OwnerStats?.SendMessage( "Door has been locked." );
			Sound.Play( "audio/lock.sound", Door.Transform.World.Position );
		}

		[Broadcast]
		private void UnlockDoor()
		{
			IsUnlocked = true;
			OwnerStats?.SendMessage( "Door has been unlocked." );
			Sound.Play( "audio/lock.sound", Door.Transform.World.Position );
		}
	}

}