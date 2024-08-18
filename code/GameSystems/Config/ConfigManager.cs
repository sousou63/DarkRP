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
}