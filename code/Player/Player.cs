using GameSystems;

namespace Sandbox.GameSystems.Player;

/// <summary>
/// Represents your local player
/// </summary>
public partial class Player : Component, Component.INetworkSpawn
{
	[Property, Group( "References" )] public PlayerHUD PlayerHud { get; set; }
	[Property, Group( "References" )] public PlayerHUD PlayerTabMenu { get; set; }
	private CameraComponent _camera;

	public string Name {get; set;} 
	
	protected override void OnAwake()
	{
		_camera = Scene.GetAllComponents<CameraComponent>().FirstOrDefault( x => x.IsMainCamera );

		if ( !Network.IsProxy )
		{
			// TODO: This should be moved off of the player and moved globally
			PlayerHud.Enabled = true;
			PlayerTabMenu.Enabled = true;
		}
	}

	protected override void OnStart()
	{
		GameController.Instance.AddPlayer( GameObject, GameObject.Network.OwnerConnection);
		Name = this.Network.OwnerConnection.DisplayName;
		
		OnStartMovement();

		if ( !Network.IsProxy )
		{
			OnStartStatus();
			OnStartInventory();
		}
	}

	protected override void OnUpdate()
	{
		OnUpdateMovement();
	}

	protected override void OnFixedUpdate()
	{
		OnFixedUpdateMovement();

		if ( !IsProxy )
		{
			OnFixedUpdateStatus();
			OnFixedUpdateInventory();
			OnFixedUpdateInteraction();
		}
	}

	public void OnNetworkSpawn( Connection owner )
	{
		OnNetworkSpawnOutfitter( owner );
	}
}
