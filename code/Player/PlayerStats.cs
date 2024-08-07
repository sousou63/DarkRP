using Sandbox;

public sealed class PlayerStats : Component
{


	// BASE PLAYER PROPERTYS

	[Property]
	public float MoneyBase { get; set; } = 500f;

	[Property]
	public float HealthBase { get; set; } = 100f;

	[Property]
	public float FoodBase { get; set; } = 100f;


	// MISC PROPERTYS

	[Property]
	public float FirstSalaryTimer { get; set; } = 3000f; // SalaryTimer = ( Fixed Update Frequency * Desired time in seconds )

	[Property]
	public float BaseSalaryTimer { get; set; } = 3000f; // SalaryTimer = ( Fixed Update Frequency * Desired time in seconds )

	[Property]
	public float SalaryAmmount { get; set; } = 50f; 


	protected override void OnUpdate()
	{

	}
}
