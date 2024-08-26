namespace Sandbox.Player.Systems;

public class Flashlight : Component
{
	[Property] private GameSystems.Player.Player _player;
	
	[Property] private SpotLight _light;
	[Property] private SoundPointComponent _soundPoint;

	private void UpdateCameraTilt()
	{
		//Updates Tilt on Flashlight because parenting to camera
		//Parents all flashlights to the 1 camera there is because each client
		//Uses the same camera
		_light.Transform.Rotation = _player.EyeAngles.ToRotation();
	}

	protected override void OnFixedUpdate()
	{
		if ( Input.Pressed( "Flashlight" ) )
		{
			ToggleFlashlight();
		}
	}

	protected override void OnUpdate()
	{
		if ( _light.Enabled )
		{
			UpdateCameraTilt();
		}
	}
	
	[Broadcast(NetPermission.OwnerOnly)]
	public void ToggleFlashlight()
	{
		//Inverts the state of the light
		if ( _light == null ) return;
		_light.Enabled = !_light.Enabled;

		//Play the click click sound
		_soundPoint.StartSound();
	}

}
