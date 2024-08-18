using System;
using GameSystems.Player;

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
						permissionLevel: PermissionLevel.User,
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
						permissionLevel: PermissionLevel.User,
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
						permissionLevel: PermissionLevel.Admin,
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

								var GameController = GameSystems.GameController.Instance;
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
						permissionLevel: PermissionLevel.Admin,
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

								var GameController = GameSystems.GameController.Instance;
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
				)},
				{ "setrank", new Command(
						name: "setrank",
						description: "Set a player's rank",
						permissionLevel: PermissionLevel.SuperAdmin,
						commandFunction: (player, scene, args) =>
						{
								// Get the player stats
								var playerStats = player.Components.Get<Stats>();
								if (playerStats == null) return false;

								// Get the 2nd parameter for player
								if (args.Length < 2)
								{
									playerStats.SendMessage("Usage: /setrank <player> <rank>");
									return false;
								}

								var GameController = GameSystems.GameController.Instance;
								if (GameController == null) return false;

								var foundPlayer = GameController.PlayerLookup(args[0]);

								if (foundPlayer == null)
								{
									playerStats.SendMessage($"Player {args[0]} not found");
									return false;
								}

								// Get the rank
								var rank = GameController.GetUserGroup(args[1]);
								if (rank == null)
								{
									playerStats.SendMessage("Invalid rank");
									return false;
								}

								// Check if the player has permission to set the rank
								if ( playerStats.GetPlayerDetails()?.CheckPermission(rank.PermissionLevel) == false )
								{
									playerStats.SendMessage("You do not have permission to set this rank.");
									return false;
								}

								// Set the rank
								foundPlayer.SetRank(rank);

								if ( foundPlayer.GameObject != player ) foundPlayer.GameObject.Components.Get<Stats>()?.SendMessage($"Your rank has been set to {rank.DisplayName}.");
								playerStats.SendMessage($"Set {foundPlayer.Connection.DisplayName} rank to {rank.DisplayName}");
								return true;
						}
				)},
				{ "noclip", new Command(
						name: "noclip",
						description: "Enable noclip on a player",
						permissionLevel: PermissionLevel.Admin,
						commandFunction: (player, scene, args) =>
						{
							
							var targetPlayer = player;
								// Get the player stats
								var GameController = GameSystems.GameController.Instance;
								if (GameController == null) return false;
								if (args.Length > 0)
								{
									var foundPlayer = GameController.PlayerLookup(args[0]);
									if ( foundPlayer is not null ) targetPlayer = foundPlayer.GameObject;
								}
								

								// Get the player controller
								var controller = targetPlayer.Components.Get<Player.MovementController>();
								if (controller == null) return false;

								controller.ToggleNoClip(!controller.IsNoClip);
								if ( targetPlayer.Id == player.Id )
								{
									targetPlayer.Components.Get<Stats>()?.SendMessage($"Noclip {(controller.IsNoClip ? "enabled" : "disabled")}.");
								}else
								{
									player.Components.Get<Stats>()?.SendMessage($"Noclip {(controller.IsNoClip ? "enabled" : "disabled")} for {targetPlayer.Name}.");
								}
								return true;
						}
				)},
				{ "dropmoney", new Command(
						name: "dropmoney",
						description: "Drops the specified amount of money.",
						permissionLevel: PermissionLevel.User,
						commandFunction: (player, scene, args) =>
						{
								// Get the player stats
								var playerStats = player.Components.Get<Stats>();
								if (playerStats == null)
								{
									Log.Error("Player stats not found.");
									return false;
								}

								// Validate the command arguments
								if (args.Length < 1)
								{
									playerStats.SendMessage("Usage: /dropmoney <amount>");
									return false;
								}

								if (!int.TryParse(args[0], out int amount) || amount <= 0)
								{
									playerStats.SendMessage("Invalid amount specified.");
									return false;
								}

								// Check if the player has enough money to drop
								if (!playerStats.RemoveMoney(amount))
								{
									playerStats.SendMessage("You do not have enough money to drop that amount.");
									return false;
								}

								try
								{
									// Get the ConfigManager to access the MoneyPrefab
									var configManager = ConfigManager.Instance;
									if (configManager == null || configManager.MoneyPrefab == null)
									{
										Log.Error("Money prefab is not set in the ConfigManager.");
										return false;
									}

									// Clone the MoneyPrefab and position it
									var moneyObject = configManager.MoneyPrefab.Clone(player.Transform.Position);
									if (moneyObject == null)
									{
										Log.Error("Failed to clone MoneyPrefab.");
										return false;
									}

									// Attach the Money component to the GameObject
									var moneyComponent = moneyObject.Components.Get<Money>();
									if (moneyComponent == null)
									{
										Log.Error("Money component is missing on the prefab.");
										return false;
									}

									// Set the amount and owner
									moneyComponent.Amount = amount;
									moneyComponent.Owner = playerStats.GetPlayerDetails();

									// Network the spawned GameObject
									moneyObject.NetworkSpawn();

									// Notify the player
									playerStats.SendMessage($"You have dropped ${amount:N0}.");

									return true;
								}
								catch (Exception e)
								{
									Log.Error($"Error in /dropmoney command: {e.Message}");
									return false;
								}
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
			// Get the PlayerStats component. This is required for all players. Verifies the player is a player.
			var playerStats = player.Components.Get<Stats>();
			if ( playerStats == null ) return false;
			try
			{

				// Check if its the default "help" command
				if ( commandName == "help" )
				{
					var commandNames = string.Join( ", ", GetCommandNames().Select( name => "/" + name ) );

					playerStats.SendMessage( $"Available commands: {commandNames}" );
					return true;
				}

				// Get the player details
				var details = playerStats.GetPlayerDetails();
				if ( details == null ) return false;

				var command = GetCommand( commandName );

				if ( !details.CheckPermission(command.PermissionLevel) )
				{
					playerStats.SendMessage( "You do not have permission to execute this command." );
					return false;
				}

				Log.Info( $"Executing command \"{commandName}\"." );
				if ( command.CommandFunction( player, scene, args ) == false )
				{
					return false;
				}
				return true;
			}
			catch ( Exception e )
			{
				Log.Error( $"Failed to execute command \"{commandName}\": {e.Message}" );
				playerStats.SendMessage( $"Failed to execute command \"{commandName}\"." );
				return false;
			}
		}
	}
}


