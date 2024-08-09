using Sandbox;
using System;

public sealed class PlayerStats : Component
{


	// BASE PLAYER PROPERTYS

	[Property] public float MoneyBase { get; set; } = 500f;

	[Property] public float HealthBase { get; set; } = 100f;

	[Property] public float FoodBase { get; set; } = 100f;


	// TIMER PROPERTYS

	[Property] public float SalaryTimer { get; set; } = 60f; // SalaryTimer in seconds

	[Property] public float SalaryAmmount { get; set; } = 50f;


	TimeSince lastUsed = 0; // Set the timer

	protected override void OnFixedUpdate()
	{

		if ( lastUsed >= SalaryTimer )
		{
			MoneyBase += SalaryAmmount; // add Salary to the player Money
			Sound.Play( "sounds/kenney/ui/ui.upvote.sound" ); // play a basic ui sound
			lastUsed = 0; // reset the timer
		}

	}

	public bool RemoveMoney (float Ammount)
	{
		if ( MoneyBase < Ammount )
		{
			return false; // Not enough money 
		}

		else if ( MoneyBase >= Ammount )
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
}

