using Sandbox;
using System.Diagnostics;

public sealed class PrinterLogic : Component
{

	// Printer Color

	[Property] public Color Bronze { get; set; }
	[Property] public Color Silver { get; set; }
	[Property] public Color Gold { get; set; }
	[Property] public Color Diamond { get; set; }

	// Printer Price

	[Property] public float BronzePrice { get; set; }
	[Property] public float SilverPrice { get; set; }
	[Property] public float GoldPrice { get; set; }
	[Property] public float DiamondPrice { get; set; }


	// Printer Timer Setup

	[Property, Sync,] public float PrinterCurrentMoney { get; set; } = 0f;


	[Property] public float PrinterTimer { get; set; } = 60f; // Printer Timer to get the money ( in seconds )

	[Property] public float PrinterTimerMoney { get; set; } = 25f; 

	TimeSince lastUsed = 0; // Set the timer

	protected override void OnFixedUpdate()
	{

		if ( lastUsed >= PrinterTimer )
		{
			PrinterCurrentMoney += PrinterTimerMoney; // add money to the printer
			// Log.Info( PrinterCurrentMoney );
			lastUsed = 0; // reset the timer
		} 

	}
}
