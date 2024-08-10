using Commands;

public sealed class ConfigManager : Component
{
	public CommandConfig Commands { get; } = new();

	protected override void OnStart()
	{
		
	}
}