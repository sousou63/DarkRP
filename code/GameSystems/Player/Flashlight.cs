using GameSystems.Player;
using Sandbox;
using System.Diagnostics;
using System.Numerics;

public sealed class Flashlight : Component
{
	[Property] private SpotLight light;
	[Property] private SoundPointComponent soundPoint;
	[Property] private MovementController mc;

	private void UpdateCameraTilt()
	{
		//Updates Tilt on Flashlight because parenting to camera
		//Parents all flashlights to the 1 camera there is because each client
		//Uses the same camera
		light.Transform.Rotation = mc.EyeAngles.ToRotation();
	}

	protected override void OnFixedUpdate()
	{
		if ( Input.Pressed( "Flashlight" ) ) ToggleFlashlight();
	}
	protected override void OnUpdate()
	{
		if(light.Enabled) UpdateCameraTilt();
	}

	[Broadcast(NetPermission.OwnerOnly)]
	public void ToggleFlashlight()
	{
		//Inverts the state of the light
		if ( light == null ) return;
		light.Enabled = !light.Enabled;

		//Play the click click sound
		soundPoint.StartSound();
	}

}
