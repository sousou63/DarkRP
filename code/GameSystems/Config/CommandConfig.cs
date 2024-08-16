using System;
using GameSystems.Player;
using System.Security.Cryptography.X509Certificates;

namespace GameSystems.Config
{


	/// <summary>
	/// Command configuration.
	/// </summary>
	public class CommandConfig
	{
		private readonly Dictionary<string, ICommandConfig> _commands = new()
		{
			{ "clear", new Command(
						name: "clear",
						description: "Clears the chat",
						permissionLevel: 0,
						commandFunction: (player, scene, args) =>
						{
							var playerStats = player.Components.Get<Stats>();
							if (playerStats == null) return false;
							
							// Get the chat
							var chat = scene.Directory.FindByName("Screen")?.First()?.Components.Get<Chat>();
							if (chat == null) return false;

							chat.ClearChat();

							playerStats.SendMessage("Chat has been cleared");
							return true;
						}
				)},
			{ "lorem", new Command(
						name: "lorem",
						description: "Spams the chat with lorem ipsum X times.",
						permissionLevel: 0,
						commandFunction: (player, scene, args) =>
						{
								// Get the player stats
								var playerStats = player.Components.Get<Stats>();
								if (playerStats == null) return false;

								playerStats.SendMessage("Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.");
								return true;
						}
				)},
				{ "givemoney", new Command(
						name: "givemoney",
						description: "Gives the player money",
						permissionLevel: 0, // TODO make it admin
						commandFunction: (player, scene, args) =>
						{
								// Get the player stats
								var playerStats = player.Components.Get<Stats>();
								if (playerStats == null) return false;

								// Get the 2nd parameter for player
								if (args.Length < 2)
								{
									playerStats.SendMessage("Usage: /givemoney <player> <amount>");
									return false;
								}

								var amount = 0;
								if (!int.TryParse(args[1], out amount))
								{
									playerStats.SendMessage("Invalid amount");
									return false;
								}

								var GameController = ConfigManagerHelper.GetGameController(scene);
								if (GameController == null) return false;

								var foundPlayer = GameController.PlayerLookup(args[0]);

								if (foundPlayer == null)
								{
									playerStats.SendMessage($"Player {args[0]} not found");
									return false;
								}

								foundPlayer.GameObject.Components.Get<Stats>()?.AddMoney(amount);

								if ( foundPlayer.GameObject != player ) foundPlayer.GameObject.Components.Get<Stats>()?.SendMessage($"You were given ${amount.ToString("N0")} money.");
								playerStats.SendMessage($"Gave {foundPlayer.Connection.DisplayName} ${amount.ToString("N0")} money");
								return true;
						}
				)},
				{ "setmoney", new Command(
						name: "setmoney",
						description: "Set a player's money",
						permissionLevel: 0, // TODO make it admin
						commandFunction: (player, scene, args) =>
						{
								// Get the player stats
								var playerStats = player.Components.Get<Stats>();
								if (playerStats == null) return false;

								// Get the 2nd parameter for player
								if (args.Length < 2)
								{
									playerStats.SendMessage("Usage: /setmoney <player> <amount>");
									return false;
								}

								var amount = 0;
								if (!int.TryParse(args[1], out amount))
								{
									playerStats.SendMessage("Invalid amount");
									return false;
								}

								var GameController = ConfigManagerHelper.GetGameController(scene);
								if (GameController == null) return false;

								var foundPlayer = GameController.PlayerLookup(args[0]);

								if (foundPlayer == null)
								{
									playerStats.SendMessage($"Player {args[0]} not found");
									return false;
								}

								foundPlayer.GameObject.Components.Get<Stats>()?.SetMoney(amount);

								if ( foundPlayer.GameObject != player ) foundPlayer.GameObject.Components.Get<Stats>()?.SendMessage($"Your money has been set to ${amount.ToString("N0")}.");
								playerStats.SendMessage($"Set {foundPlayer.Connection.DisplayName} money to ${amount.ToString("N0")}");
								return true;
						}
				)}
		};

		public IReadOnlyCollection<ICommandConfig> Commands => _commands.Values;
		/// <summary>
		/// Registers a command.
		/// </summary>
		/// <param name="command"></param>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="InvalidOperationException"></exception>
		public void RegisterCommand(ICommandConfig command)
		{
			if (command == null)
				throw new ArgumentNullException(nameof(command));

			var commandNameLower = command.Name.ToLowerInvariant();
			if (_commands.ContainsKey(commandNameLower)) Log.Warning($"Command with name \"{commandNameLower}\" already exists.");

			_commands[commandNameLower] = command;
		}

		public void UnregisterCommand(string commandName)
		{
			commandName = commandName.ToLowerInvariant();
			if (string.IsNullOrWhiteSpace(commandName))
				throw new ArgumentException("Command name cannot be null or whitespace.", nameof(commandName));

			if (!_commands.Remove(commandName)) Log.Warning($"Command with name \"{commandName}\" does not exist.");
		}

		public ICommandConfig GetCommand(string commandName)
		{
			// Lowercase the command name to make it case-insensitive.
			commandName = commandName.ToLowerInvariant();
			if (string.IsNullOrWhiteSpace(commandName))
				throw new ArgumentException("Command name cannot be null or whitespace.", nameof(commandName));

			if (_commands.TryGetValue(commandName, out var command))
				return command;

			throw new KeyNotFoundException($"Command with name {commandName} does not exist.");
		}

		public string[] GetCommandNames()
		{
			var commandNames = _commands.Keys.ToList();
			commandNames.Add("help");
			return commandNames.ToArray();
		}

		public bool ExecuteCommand(string commandName, GameObject player, Scene scene, string[] args)
		{
			try
			{
				// Check if its the default "help" command
				if (commandName == "help")
				{
					var playerStats = player.Components.Get<Stats>();
					if (playerStats == null) return false;

					var commandNames = string.Join(", ", GetCommandNames().Select(name => "/" + name));

					playerStats.SendMessage($"Available commands: {commandNames}");
					return true;
				}
				var command = GetCommand(commandName);
				Log.Info($"Executing command \"{commandName}\".");
				if (command.CommandFunction(player, scene, args) == false)
				{
					Log.Error($"Failed to execute command \"{commandName}\".");
					var playerStats = player.Components.Get<Stats>();
					if (playerStats == null) return false;
					playerStats.SendMessage($"Failed to execute command \"{commandName}\".");
					return false;
				}
				return true;
			}
			catch (Exception e)
			{
				Log.Error($"Failed to execute command \"{commandName}\": {e.Message}");
				return false;
			}
		}
	}
}


