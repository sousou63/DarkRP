using System;

namespace GameSystems.Config
{
	public sealed class ConfigManager : Component
	{

		// Property for the Money Prefab
        [Property] public GameObject MoneyPrefab { get; set; }
		public CommandConfig Commands { get; } = new();

		protected override void OnStart()
		{

		}
	}

	// TODO move and refactor this to a better place. It should be more generic and used to cache and fetch all GameController components for easy lookups for other components.
	public static class ConfigManagerHelper
	{
		public static GameObject GameControllerObject { get; set; } = null;
		public static ConfigManager ConfigManager { get; set; } = null;
		public static GameController GameController { get; set; } = null;
		private static GameObject FetchCacheGameController(Scene scene)
		{
			try
			{
				if (GameControllerObject != null) return GameControllerObject;
				GameControllerObject = scene.Directory.FindByName("Game Controller")?.First();
				if (GameControllerObject == null)
				{
					Log.Error("Game Controller not found");
				}
				return GameControllerObject;
			}
			catch (Exception e)
			{
				Log.Error($"Failed to fetch game controller: {e.Message}");
				return null; // Ensure a value is returned
			}
		}

		private static bool FetchCacheComponents(Scene scene)
		{
			try
			{
				if (ConfigManager != null && GameController != null) return true;
				var controller = FetchCacheGameController(scene);
				if (controller == null) return false;
				ConfigManager = controller.Components.Get<ConfigManager>();
				if (ConfigManager == null) Log.Error("Config Manager not found");
				GameController = controller.Components.Get<GameController>();
				return true;
			}
			catch (Exception e)
			{
				Log.Error($"Failed to fetch Config Manager: {e.Message}");
				return false;
			}
		}

		public static ConfigManager GetConfigManager(Scene scene)
		{
			try
			{
				if (FetchCacheComponents(scene))
				{
					return ConfigManager;
				}
				return null;
			}
			catch (Exception e)
			{
				Log.Error($"Failed to get Config Manager: {e.Message}");
				return null;
			}
		}

		public static GameController GetGameController(Scene scene)
		{
			try
			{
				if (FetchCacheComponents(scene))
				{
					return GameController;
				}
				return null;
			}
			catch (Exception e)
			{
				Log.Error($"Failed to get Game Controller: {e.Message}");
				return null;
			}
		}
	}
}