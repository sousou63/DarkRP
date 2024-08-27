using System;
using Entity.Interactable.Door;
using GameSystems;
using GameSystems.Jobs;
using Sandbox.GameSystems.Database;
using Sandbox.UI;

namespace Sandbox.GameSystems.Player
{

	public partial class Player
	{
		// DOORS
		[Sync][Property, Group("Status")]  public List<GameObject> Doors { get; private set; } = new();

		// BASE PLAYER PROPERTYS

		[Sync, HostSync][Property, Group("Status")] public float Balance { get; set; } = 500f;

		[Property, Group("Status")] public float Health { get; private set; } = 100f;
		[Property, Group("Status")]  public float Hunger { get; private set; } = 100f;
		[Property, Group("Status")]  public float MaxHealth { get; private set; } = 100f;
		[Property, Group("Status")]  public float HungerMax { get; private set; } = 100f;
		[Property, Group("Status")]  public bool Dead { get; private set; } = false;
		[Property, Group("Status")]  public bool Starving { get; private set; } = false;

		// TIMER PROPERTYS

		[Property] public float SalaryTimerSeconds { get; set; } = 60f; // SalaryTimer in seconds
		[Property] public float StarvingTimerSeconds { get; set; } = 20f;

		private Chat chat { get; set; }
		private GameController controller { get; set; }

		TimeSince lastUsed = 0; // Set the timer
		TimeSince lastUsedFood = 0;

		//Pereodiocal player data save in seconds
		private TimeSince lastSaved = 0;
		private static uint saveCooldown = 30;
		// TODO add a "/sellallowneddoors" command to sell all doors owned by the player

		private void OnStartStatus()
		{
			chat = Scene.Directory.FindByName( "Screen" )?.First()?.Components.Get<Chat>();
			if ( chat is null ) Log.Error( "Chat component not found" );
			try
			{
				controller = GameController.Instance;
			}
			catch ( Exception e )
			{
				Log.Error( e );
				return;
			}
		}

		private void OnFixedUpdateStatus()
		{
			if ( lastUsed >= SalaryTimerSeconds && (Network.IsOwner) )
			{
				Balance += GameController.Instance.GetPlayerByGameObjectId( GameObject.Id ).Job.Salary; // add Salary to the player Money
				Sound.Play( "sounds/kenney/ui/ui.upvote.sound" ); // play a basic ui sound
				lastUsed = 0; // reset the timer
			}

			if ( lastSaved >= saveCooldown && (Networking.IsHost) )
			{

				if ( GetNetworkPlayer() != null )
				{
					SavedPlayer.SavePlayer( new SavedPlayer( this.GetNetworkPlayer() ) );
					lastSaved = 0; // reset the timer
				}

			}

			if ( lastUsedFood >= StarvingTimerSeconds && (Network.IsOwner) && (Starving) )
			{
				if ( Hunger > 0 )
				{
					Hunger -= 1;
				}
				lastUsedFood = 0; // reset the timer
			}
			if ( Health < 1 || Hunger < 1 )
			{
				Dead = true;
				Health = 0;
				Hunger = 0;
			}
			if ( Health > MaxHealth) {Health = MaxHealth;}
			if ( Hunger > HungerMax) {Hunger = HungerMax;}
			if ( Dead )
			{
				// TODO: Make ragdolls and die
			}
		}

		/// <summary>
		/// Helper function to find the player's PlayerDetails
		/// </summary>
		/// <returns></returns>
		public NetworkPlayer GetNetworkPlayer()
		{
			return controller.GetPlayerByGameObjectId( GameObject.Id );
		}
		
		/// <summary>
		/// Updates the player's balance. If the amount is negative, it checks if the player can afford it. Returns false if the player can't afford it.
		/// </summary>
		public bool UpdateBalance( float Amount )
		{
			// If the amount is a negative, check if the player can afford it
			if ( Amount < 0 )
			{
				if ( Balance < Math.Abs( Amount ) )
				{
					Sound.Play( "audio/error.sound" );
					return false;
				}
			}
			Balance += Amount;
			return true;
		}

		public void SetBalance( float Amount )
		{
			Balance = Amount;
		}

		public void UpdateHunger( float Amount )
		{
			Hunger += Amount;
		}
		public void SetHunger( float Amount )
		{
			Hunger = Amount;
		}

		public void SellAllDoors()
		{
			Log.Info( "Selling All " + Doors.Count + " doors" );
			for (int i = 0; i < Doors.Count; i++)
			{
				GameObject door = Doors[i];
				DoorLogic doorLogic = door.Components.Get<DoorLogic>();
				doorLogic.SellDoor(this);
			}
			SendMessage( "All doors have been sold." );
		}

		// TODO this would need to go to its own class. PlayerController or some shit
		public void SendMessage( string message )
		{
			using ( Rpc.FilterInclude( c => c.Id == GameObject.Network.OwnerId ) )
			{
				chat?.NewSystemMessage( message );
			}
		}
	}
}
