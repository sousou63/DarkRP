using Sandbox;
using GameSystems.Player;

namespace Entity.Interactable.Printer
{
	public sealed class PrinterLogic : Component, IInteractable
	{
		// Define the different types of printers
		public enum PrinterType { Bronze, Silver, Gold, Diamond };

		// PRINTER SETTINGS
		[Property] public GameObject Fan { get; set; }
		[Property] public float FanSpeed { get; set; } = 900f;
		[Property] public Color Bronze { get; set; } = Color.Orange;
		[Property] public float BronzePrice { get; set; } = 500f;
		[Property] public float BronzeTimer { get; set; } = 25f; // (in seconds)

		[Property] public Color Silver { get; set; } = Color.Gray;
		[Property] public float SilverPrice { get; set; } = 1200f;
		[Property] public float SilverTimer { get; set; } = 18f; // (in seconds)

		[Property] public Color Gold { get; set; } = Color.Yellow;
		[Property] public float GoldPrice { get; set; } = 2600f;
		[Property] public float GoldTimer { get; set; } = 13f; // (in seconds)

		[Property] public Color Diamond { get; set; } = Color.Blue;
		[Property] public float DiamondPrice { get; set; } = 4800f;
		[Property] public float DiamondTimer { get; set; } = 8f; // (in seconds)

		// Printer Timer Setup
		[Property, Sync] public float PrinterCurrentMoney { get; set; } = 0f;
		[Property] public float PrinterTimerMoney { get; set; } = 25f;
		[Property] public float PrinterMaxMoney { get; set; } = 8000f;

		private TimeSince lastUsed = 0; // Set the timer
		private PrinterType currentPrinterType; // Store the current printer type

		/// <summary>
		/// Interact with the printer. This comes from the IInteractable interface inherited from the Interactable class.
		/// </summary>
		public void InteractUse( SceneTraceResult tr, GameObject player )
		{
			Log.Info( "Interacting with printer" );
			if ( PrinterCurrentMoney > 0 )
			{
				var playerStats = player.Components.Get<Stats>();
				if ( playerStats == null ) {  return; }

				playerStats.AddMoney( PrinterCurrentMoney );
				ResetPrinterMoney();
				Sound.Play( "audio/money.sound" );
			}
		}

		protected override void OnFixedUpdate()
		{
			// Determine the timer based on the printer type
			float printerTimer = GetPrinterTimer();
			Fan.Transform.Rotation = Rotation.FromPitch(Time.Now * FanSpeed);
			
			// If the timer has passed, add money
			if ( lastUsed >= printerTimer )
			{
				if ( PrinterCurrentMoney < PrinterMaxMoney )
				{
					PrinterCurrentMoney += PrinterTimerMoney; // Add money to the printer
				}

				lastUsed = 0; // Reset the timer
			}
		}

		// Method to set the current printer type and update its color
		public void SetPrinterType( PrinterType type )
		{
			currentPrinterType = type;
			// Automatically update the color when the printer type is set
			UpdatePrinterColor(); 
		}

		[Broadcast]
		public void ResetPrinterMoney()
		{
			PrinterCurrentMoney = 0f;
		}
		// Method to get the correct timer based on the printer type
		private float GetPrinterTimer()
		{
			switch ( currentPrinterType )
			{
				case PrinterType.Bronze:
					return BronzeTimer;
				case PrinterType.Silver:
					return SilverTimer;
				case PrinterType.Gold:
					return GoldTimer;
				case PrinterType.Diamond:
					return DiamondTimer;
				default:
					return 60f; // Default timer, in case something goes wrong
			}
		}

		// Method to update the printer color based on the printer type
		private void UpdatePrinterColor()
		{
			Color newColor;

			switch ( currentPrinterType )
			{
				case PrinterType.Bronze:
					newColor = Bronze;
					break;
				case PrinterType.Silver:
					newColor = Silver;
					break;
				case PrinterType.Gold:
					newColor = Gold;
					break;
				case PrinterType.Diamond:
					newColor = Diamond;
					break;
				default:
					newColor = Color.White; // Default color, in case something goes wrong
					break;
			}

			// Assuming there's a component responsible for rendering the model
			this.Components.Get<ModelRenderer>().Tint = newColor;
		}

	}
}
