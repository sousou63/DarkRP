using System;
using GameSystems;
using Entity.Interactable.Door;

namespace GameSystems.Player
{

	public sealed class Stats : Component
	{
		// DOORS

		public List<GameObject> Doors { get; private set; } = new List<GameObject>();

		// BASE PLAYER PROPERTYS

		[Property] public float MoneyBase { get; set; } = 500f;

		[Property] public float HealthBase { get; set; } = 100f;
		[Property] public bool Starving { get; set; } = false;
		[Property] public float FoodBase { get; set; } = 100f;
		[Property] public bool Died { get; set; } = false;

		// TIMER PROPERTYS

		[Property] public float SalaryTimer { get; set; } = 60f; // SalaryTimer in seconds
		[Property] public float StarvingTimer { get; set; } = 20f;
		[Property] public float SalaryAmmount { get; set; } = 50f;

		private Chat chat { get; set; }
		private GameController controller { get; set; }

		TimeSince lastUsed = 0; // Set the timer
		TimeSince lastUsedFood = 0;
		// TODO add a "/sellallowneddoors" command to sell all doors owned by the player

		protected override void OnStart()
		{
			chat = Scene.Directory.FindByName("Screen")?.First()?.Components.Get<Chat>();
			if (chat is null) Log.Error("Chat component not found");
			try {
				controller = GameController.Instance;
				if ( controller == null ) Log.Error( "Game Controller component not found" );
				controller.AddPlayer( GameObject, GameObject.Network.OwnerConnection );
			}catch ( Exception e )
			{
				Log.Error( e );
			}
		}

		protected override void OnFixedUpdate()
		{

			if (lastUsed >= SalaryTimer && (Network.IsOwner))
			{
				MoneyBase += SalaryAmmount; // add Salary to the player Money
				Sound.Play("sounds/kenney/ui/ui.upvote.sound"); // play a basic ui sound
				lastUsed = 0; // reset the timer
			}
			if (lastUsedFood >= StarvingTimer && (Network.IsOwner) && (Starving))
			{
				if (FoodBase > 0)
				{
					FoodBase -= 1;
				}
				lastUsedFood = 0; // reset the timer
			}
			if (HealthBase < 1 || FoodBase < 1)
			{
				Died = true;
				HealthBase = 0;
				FoodBase = 0;
			}
			if (Died)
			{
				// TODO: Make ragdolls and die
			}
		}

		/// <summary>
		/// Helper function to find the player's PlayerDetails
		/// </summary>
		/// <returns></returns>
		public PlayerConnObject GetPlayerDetails()
		{
			return controller.GetPlayerByGameObjectID(GameObject.Id);
		}

		public bool RemoveMoney(float Ammount)
		{
			if (MoneyBase < Ammount)
			{
				Sound.Play("audio/error.sound");
				return false; // Not enough money 
			}
			else if (MoneyBase >= Ammount)
			{
				MoneyBase -= Ammount;
				return true; // Successfully removed money
			}
			return false;
		}

		public void AddMoney(float Ammount)
		{
			MoneyBase += Ammount;
		}

		public void SetMoney(float Ammount)
		{
			MoneyBase = Ammount;
		}
		public void AddFood(float Ammount)
		{
			FoodBase += Ammount;
		}
		public void SetFood(float Ammount)
		{
			FoodBase = Ammount;
		}
		public bool RemoveFood(float Ammount)
		{
			FoodBase -= Ammount;
			return true; // Successfully removed food
		}
		// DOOR LOGIC. Helps keep track of owned doors.

		public bool PurchaseDoor(float price, GameObject door)
		{
			// Check if its a valid door
			var doorLogic = door.Components.Get<DoorLogic>();
			if (doorLogic == null)
			{
				return false;
			}
			// Check if the door is already owned
			if (Doors.Any(d => d.Id == door.Id))
			{
				return false;
			}

			// If the player can afford it
			if (RemoveMoney(price))
			{
				Doors.Add(door);
				doorLogic.PurchaseDoor(GameObject);
				SendMessage("The door has been purchased.");
				return true;
			}
			else
			{
				SendMessage("Can't afford this door.");
				return false;
			}

		}

		public bool SellDoor(GameObject door)
		{
			// Check if its a valid door
			var doorLogic = door.Components.Get<DoorLogic>();
			if (doorLogic == null)
			{
				return false;
			}

			// Check if the door is owned
			if (!Doors.Any(d => d.Id == door.Id))
			{
				return false;
			}

			// Remove the door from the list
			Doors.Remove(door);
			doorLogic.SellDoor();
			SendMessage("Door has been sold.");
			return true;
		}

		public void SellAllDoors()
		{
			Log.Info("Selling all doors");
			foreach (var door in Doors)
			{
				Log.Info($"Selling door: {door.Id}");
				SellDoor(door);
			}
			SendMessage("All doors have been sold.");
		}

		// TODO this would need to go to its own class. PlayerController or some shit
		public void SendMessage(string message)
		{
			using (Rpc.FilterInclude(c => c.Id == Rpc.CallerId))
			{
				// Send the message
				chat?.NewSystemMessage(message);
			}
		}
	}

}
