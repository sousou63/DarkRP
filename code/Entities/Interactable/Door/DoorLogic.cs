using GameSystems;
using GameSystems.Player;
using Sandbox.Entity;
using Sandbox.GameSystems.Player;

namespace Entity.Interactable.Door
{
	public sealed class DoorLogic : BaseEntity, Component.INetworkListener
	{
		[Property] public GameObject Door { get; set; }
		[Property, Sync] public bool IsUnlocked { get; set; } = true;
		[Property, Sync]public bool IsOpen { get; set; } = false;
		public Player OwnerStats { get; set; }

		[Property, Sync] public int Price { get; set; } = 100;

		[Property] DoorMenu DoorMenu {get; set;}
		public string DoorTitle = ""; 


		public override void InteractUse( SceneTraceResult tr, GameObject player )
		{
			// Dont interact with the door if it is locked
			if ( IsUnlocked == false ) return;

			// Open / Close door
			OpenCloseDoor();
		}
		public override void InteractSpecial( SceneTraceResult tr, GameObject player )
		{
			if ( Owner == null )
			{
				PurchaseDoor(player, player.Components.Get<Player>());
				return;
			}

			if (player.Id == Owner?.GameObject.Id)
			{
				DoorMenu.OpenDoorMenu(this.Door, player);
			}
		}

		public override void InteractAttack1( SceneTraceResult tr, GameObject player )
		{
			// TODO The user should have a "keys" weapon select to do the following interactions to avoid input conflicts
			if (player.Id == Owner?.GameObject.Id ) { LockDoor(); } else { KnockOnDoor(); }
		}

		public override void InteractAttack2( SceneTraceResult tr, GameObject player )
		{
			// TODO The user should have a "keys" weapon select to do the following interactions to avoid input conflicts
			if (player.Id == Owner?.GameObject.Id) { UnlockDoor(); } else { KnockOnDoor(); }
		}

		[Broadcast]
		public void UpdateDoorOwner( GameObject player = null, Player playerStats = null )
		{
			Owner = player != null ? GameController.Instance.GetPlayerByGameObjectId( player.Id ) : null;
			OwnerStats = playerStats;
		}

		public void PurchaseDoor(GameObject player, Player playerStats)
		{
			if (playerStats.UpdateBalance(-Price))
			{
				Sound.Play( "audio/notification.sound" );
				playerStats.Doors.Add(Door);
				UpdateDoorOwner(player, playerStats);
			}
		}

		public void SellDoor(Player playerStats) //This Function does no longer removes the Door in Player.Stats or checks if it's done
		{
			IsUnlocked = true;
			playerStats.Doors.Remove(this.Door);
			playerStats.UpdateBalance(Price / 2);
			DoorTitle = "";

			UpdateDoorOwner();
		}

		public void SetDoorTitle(string title)
		{
			DoorTitle = title;
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
		public void LockDoor()
		{
			IsUnlocked = false;
			OwnerStats?.SendMessage( "Door has been locked." );
			Sound.Play( "audio/lock.sound", Door.Transform.World.Position );
		}

		[Broadcast]
		public void UnlockDoor()
		{
			IsUnlocked = true;
			OwnerStats?.SendMessage( "Door has been unlocked." );
			Sound.Play( "audio/lock.sound", Door.Transform.World.Position );
		}
		private void KnockOnDoor()
		{
			Sound.Play( "audio/knock.sound", Door.Transform.World.Position );
		}
	}
}
