using GameSystems.Config;

namespace Sandbox.GameSystems.Config;

public sealed class ConfigManager : Component
{
	private static ConfigManager _instance;
	// Property for the Money Prefab
	[Property] public GameObject MoneyPrefab { get; set; }
	[Sync] public CommandConfig Commands { get; } = new();
	public ConfigManager()
	{
		if ( _instance != null )
		{
			Log.Warning( "Only one instance of ConfigManager is allowed." );
		}
		_instance = this;
	}
	public static ConfigManager Instance => _instance;

	protected override void OnStart()
	{

	}
}
