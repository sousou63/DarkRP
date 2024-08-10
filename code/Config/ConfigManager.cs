using Commands;

public sealed class ConfigManager : Component
{
	public CommandConfig Commands { get; } = new();

	protected override void OnStart()
	{
		Commands.RegisterCommand(new Command(
				name: "help",
				description: "List all available commands",
				permissionLevel: 0,
				commandFunction: (args) =>
				{
						var commandNames = Commands.GetCommandNames();
						Log.Info($"Available commands: {string.Join(", ", commandNames)}");
						return true;
				}
		));
	}
}