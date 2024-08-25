using GameSystems.Player;
using Sandbox;
using System.Diagnostics;
using System.Numerics;

public sealed class Flashlight : Component
{
	[Property] private SpotLight light;

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

		//Play the click click sound
		Sound.Play( "audio/flashlighton.sound", this.Transform.World.Position );
	}

}
