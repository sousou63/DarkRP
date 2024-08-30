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
		[Sync][Property, Group("Status")]  public List<GameObject> Doors { get; private set; } = new();
		[Sync, HostSync][Property, Group("Status")] public float Balance { get; set; } = 500f;
		[Property, Group("Status")] public float Health { get; private set; } = 100f;
		[Property, Group("Status")]  public float Hunger { get; private set; } = 100f;
		[Property, Group("Status")]  public float MaxHealth { get; private set; } = 100f;
		[Property, Group("Status")]  public float HungerMax { get; private set; } = 100f;
		[Property, Group("Status")]  public bool Dead { get; private set; } = false;
		[Property, Group("Status")]  public bool Starving { get; private set; } = false;
		[Property] private float _salaryTimerSeconds { get; set; } = 60f; // SalaryTimer in seconds
		[Property] private float _starvingTimerSeconds { get; set; } = 20f;
		private Chat _chat { get; set; }
		private GameController _controller { get; set; }
		private static readonly uint _saveCooldown = 30;
		private TimeSince _lastUsed = 0; // Set the timer
		private TimeSince _lastUsedFood = 0;
		//Pereodiocal player data save in seconds
		private TimeSince _lastSaved = 0;

		// TODO add a "/sellallowneddoors" command to sell all doors owned by the player

		private void OnStartStatus()
		{
			_chat = Scene.Directory.FindByName( "Screen" )?.First()?.Components.Get<Chat>();
			if ( _chat is null ) { Log.Error( "Chat component not found" ); }
			_controller = GameController.Instance;
		}

		private void OnFixedUpdateStatus()
		{
			if ( _lastUsed >= _salaryTimerSeconds && (Network.IsOwner) )
			{
				Balance += GetNetworkPlayer().Job.Salary; // add Salary to the player Money
				Sound.Play( "sounds/kenney/ui/ui.upvote.sound" ); // play a basic ui sound
				_lastUsed = 0; // reset the timer
			}

			if ( _lastSaved >= _saveCooldown && (Networking.IsHost) )
			{

				if ( GetNetworkPlayer() != null )
				{
					SavedPlayer.SavePlayer( new SavedPlayer( this.GetNetworkPlayer() ) );
					_lastSaved = 0; // reset the timer
				}

			}

			if ( _lastUsedFood >= _starvingTimerSeconds && (Network.IsOwner) && (Starving) )
			{
				if ( Hunger > 0 )
				{
					Hunger -= 1;
				}
				_lastUsedFood = 0; // reset the timer
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
			return _controller.GetPlayerByGameObjectId( GameObject.Id );
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
			foreach ( var door in Doors )
			{
				DoorLogic doorLogic = door.Components.Get<DoorLogic>();
				doorLogic.SellDoor(this);
			}
			SendMessage( "All doors have been sold." );
		}

		public void TakeDoorOwnership( GameObject door )
		{
			door.Network.TakeOwnership();

			Log.Info( $"TakeOwnership to door : {door}" );

		}

		public void DropDoorOwnership( GameObject door )
		{
			door.Network.DropOwnership();

			Log.Info( $"DropOwnership to door : {door}" );
		}

		// TODO this would need to go to its own class. PlayerController or some shit
		public void SendMessage( string message )
		{
			using ( Rpc.FilterInclude( c => c.Id == GameObject.Network.OwnerId ) )
			{
				_chat?.NewSystemMessage( message );
			}
		}
	}
}
