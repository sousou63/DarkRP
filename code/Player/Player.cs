using GameSystems;
using Sandbox.UI;

namespace Sandbox.GameSystems.Player;

/// <summary>
/// Represents your local player
/// </summary>
public partial class Player : Component, Component.INetworkSpawn
{
	private CameraComponent _camera;
	
	
	[Property, Group("References")] public PlayerHUD PlayerHud { get; set; }
	[Property, Group("References")] public PlayerHUD PlayerTabMenu { get; set; }
	[Property, Group("References")] public LeaderBoard LeaderBoard { get; set; }

	
	protected override void OnAwake()
	{
		_camera = Scene.GetAllComponents<CameraComponent>().FirstOrDefault( x => x.IsMainCamera );

		if ( !Network.IsProxy )
		{
			// TODO: This should be moved off of the player and moved globally
			PlayerHud.Enabled = true;
			PlayerTabMenu.Enabled = true;
			LeaderBoard.Enabled = true;
		}
	}

	protected override void OnStart()
	{
		GameController.Instance.AddPlayer( GameObject, GameObject.Network.OwnerConnection);
		
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
		OnNetworkSpawnOutfitter(owner);
	}
}
