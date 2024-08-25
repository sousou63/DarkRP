using GameSystems.Player;
using Sandbox;
using System.Diagnostics;
using System.Numerics;

public sealed class Flashlight : Component
{
	[Property]
	private SpotLight light;
	[Property]
	private SoundPointComponent soundPoint;

	protected override void OnFixedUpdate()
	{
		if ( Input.Pressed( "Flashlight" ) ) ToggleFlashlight();
	}

	[Broadcast(NetPermission.OwnerOnly)]
	public void ToggleFlashlight()
	{
		//Inverts the state of the light
		if ( light == null ) return;
		light.Enabled = !light.Enabled;

		PlaySound();
		
	}

	/// <summary>
	/// Plays Toggle Audio dependant on state
	/// </summary>
	[Broadcast(NetPermission.OwnerOnly)]
	private void PlaySound()
	{
		//Selects what file to play based on state
		string soundToPlay = light.Enabled ? "audio/FlashlightOn.sound" : "audio/FlashlightOff.sound";

		SoundEvent soundEvent = new SoundEvent( soundToPlay );
		if ( soundEvent == null ) return;

		soundPoint.SoundEvent = soundEvent;

		//Stops the last audio so the new one can play if needed
		soundPoint.StopSound();

		soundPoint.StartSound();
	}
}
