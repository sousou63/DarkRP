using System;
using PlayerDetails;
using System.Security.Cryptography.X509Certificates;
using Commands;

namespace Commands
{
	/// <summary>
	/// Interface for command configuration.
	/// </summary>
	public interface ICommandConfig
	{
		string Name { get; }
		string Description { get; }
		int PermissionLevel { get; }
		bool CommandFunction( GameObject player, Scene scene, string[] args );
	}

	/// <summary>
	/// Represents a command with a name, description, permission level, and a function to execute.
	/// </summary>
	public class Command : ICommandConfig
	{
		/// <summary>
		/// Gets the name of the command.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// Gets the description of the command.
		/// </summary>
		public string Description { get; }

		/// <summary>
		/// Gets the permission level required to execute the command. Currently does nothing.
		/// </summary>
		public int PermissionLevel { get; } = 0;

		/// <summary>
		///  The function to execute when the command is called.
		/// </summary>
		private readonly Func<GameObject, Scene, string[], bool> commandFunction;

		/// <summary>
		/// Initializes a new instance of the Command class.
		/// </summary>
		/// <param name="name">The name of the command.</param>
		/// <param name="description">The description of the command.</param>
		/// <param name="permissionLevel">The permission level required to execute the command.</param>
		/// <param name="commandFunction">The function to execute when the command is called.</param>
		/// <exception cref="ArgumentNullException">
		/// Thrown when <paramref name="name"/>, <paramref name="description"/>, or <paramref name="commandFunction"/> is null.
		/// </exception>
		public Command( string name, string description, int permissionLevel, Func<GameObject, Scene, string[], bool> commandFunction )
		{
			Name = name.ToLowerInvariant() ?? throw new ArgumentNullException( nameof( name ) );
			Description = description ?? throw new ArgumentNullException( nameof( description ) );
			PermissionLevel = permissionLevel;
			this.commandFunction = commandFunction ?? throw new ArgumentNullException( nameof( commandFunction ) );
		}

		/// <summary>
		/// Executes the command function with the provided arguments.
		/// </summary>
		public bool CommandFunction( GameObject player, Scene scene, string[] args = null ) => commandFunction( player, scene, args );
	}

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
							var playerStats = player.Components.Get<PlayerStats>();
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
								var playerStats = player.Components.Get<PlayerStats>();
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
								var playerStats = player.Components.Get<PlayerStats>();
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

								foundPlayer.GameObject.Components.Get<PlayerStats>()?.AddMoney(amount);

								if ( foundPlayer.GameObject != player ) foundPlayer.GameObject.Components.Get<PlayerStats>()?.SendMessage($"You were given ${amount.ToString("N0")} money.");
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
								var playerStats = player.Components.Get<PlayerStats>();
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

								foundPlayer.GameObject.Components.Get<PlayerStats>()?.SetMoney(amount);

								if ( foundPlayer.GameObject != player ) foundPlayer.GameObject.Components.Get<PlayerStats>()?.SendMessage($"Your money has been set to ${amount.ToString("N0")}.");
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
		public void RegisterCommand( ICommandConfig command )
		{
			if ( command == null )
				throw new ArgumentNullException( nameof( command ) );

			var commandNameLower = command.Name.ToLowerInvariant();
			if ( _commands.ContainsKey( commandNameLower ) ) Log.Warning( $"Command with name \"{commandNameLower}\" already exists." );

			_commands[commandNameLower] = command;
		}

		public void UnregisterCommand( string commandName )
		{
			commandName = commandName.ToLowerInvariant();
			if ( string.IsNullOrWhiteSpace( commandName ) )
				throw new ArgumentException( "Command name cannot be null or whitespace.", nameof( commandName ) );

			if ( !_commands.Remove( commandName ) ) Log.Warning( $"Command with name \"{commandName}\" does not exist." );
		}

		public ICommandConfig GetCommand( string commandName )
		{
			// Lowercase the command name to make it case-insensitive.
			commandName = commandName.ToLowerInvariant();
			if ( string.IsNullOrWhiteSpace( commandName ) )
				throw new ArgumentException( "Command name cannot be null or whitespace.", nameof( commandName ) );

			if ( _commands.TryGetValue( commandName, out var command ) )
				return command;

			throw new KeyNotFoundException( $"Command with name {commandName} does not exist." );
		}

		public string[] GetCommandNames()
		{
			var commandNames = _commands.Keys.ToList();
			commandNames.Add( "help" );
			return commandNames.ToArray();
		}

		public bool ExecuteCommand( string commandName, GameObject player, Scene scene, string[] args )
		{
			try
			{
				// Check if its the default "help" command
				if ( commandName == "help" )
				{
					var playerStats = player.Components.Get<PlayerStats>();
					if ( playerStats == null ) return false;

					var commandNames = string.Join( ", ", GetCommandNames().Select( name => "/" + name ) );

					playerStats.SendMessage( $"Available commands: {commandNames}" );
					return true;
				}
				var command = GetCommand( commandName );
				Log.Info( $"Executing command \"{commandName}\"." );
				if ( command.CommandFunction( player, scene, args ) == false )
				{
					Log.Error( $"Failed to execute command \"{commandName}\"." );
					var playerStats = player.Components.Get<PlayerStats>();
					if ( playerStats == null ) return false;
					playerStats.SendMessage( $"Failed to execute command \"{commandName}\"." );
					return false;
				}
				return true;
			}
			catch ( Exception e )
			{
				Log.Error( $"Failed to execute command \"{commandName}\": {e.Message}" );
				return false;
			}
		}
	}
}


// TODO move and refactor this to a better place. It should be more generic and used to cache and fetch all GameController components for easy lookups for other components.
public static class ConfigManagerHelper
{
	public static GameObject GameControllerObject { get; set; } = null;
	public static ConfigManager ConfigManager { get; set; } = null;
	public static GameController GameController { get; set; } = null;
	private static GameObject FetchCacheGameController( Scene scene )
	{
		try
		{
			if ( GameControllerObject != null ) return GameControllerObject;
			GameControllerObject = scene.Directory.FindByName( "Game Controller" )?.First();
			if ( GameControllerObject == null )
			{
				Log.Error( "Game Controller not found" );
			}
			return GameControllerObject;
		}
		catch ( Exception e )
		{
			Log.Error( $"Failed to fetch game controller: {e.Message}" );
			return null; // Ensure a value is returned
		}
	}

	private static bool FetchCacheComponents( Scene scene )
	{
		try
		{
			if ( ConfigManager != null && GameController != null ) return true;
			var controller = FetchCacheGameController( scene );
			if ( controller == null ) return false;
			ConfigManager = controller.Components.Get<ConfigManager>();
			if ( ConfigManager == null ) Log.Error( "Config Manager not found" );
			GameController = controller.Components.Get<GameController>();
			return true;
		}
		catch ( Exception e )
		{
			Log.Error( $"Failed to fetch Config Manager: {e.Message}" );
			return false;
		}
	}

	public static ConfigManager GetConfigManager( Scene scene )
	{
		try
		{
			if ( FetchCacheComponents( scene ) )
			{
				return ConfigManager;
			}
			return null;
		}
		catch ( Exception e )
		{
			Log.Error( $"Failed to get Config Manager: {e.Message}" );
			return null;
		}
	}

	public static GameController GetGameController( Scene scene )
	{
		try
		{
			if ( FetchCacheComponents( scene ) )
			{
				return GameController;
			}
			return null;
		}
		catch ( Exception e )
		{
			Log.Error( $"Failed to get Game Controller: {e.Message}" );
			return null;
		}
	}
}
