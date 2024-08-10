using System;

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
    bool CommandFunction( string[] args );
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
      private readonly Func<string[], bool> commandFunction;
  
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
      public Command(string name, string description, int permissionLevel, Func<string[], bool> commandFunction)
      {
          Name = name.ToLowerInvariant() ?? throw new ArgumentNullException(nameof(name));
          Description = description ?? throw new ArgumentNullException(nameof(description));
          PermissionLevel = permissionLevel;
          this.commandFunction = commandFunction ?? throw new ArgumentNullException( nameof( commandFunction ) );
      }
  
      /// <summary>
      /// Executes the command function with the provided arguments.
      /// </summary>
      /// <param name="args">The arguments to pass to the command function. Can be null.</param>
      /// <returns>True if the command executed successfully; otherwise, false.</returns>
      public bool CommandFunction(string[] args = null) => commandFunction(args);
  }

  /// <summary>
  /// Command configuration.
  /// </summary>
  public class CommandConfig
  {
    private readonly Dictionary<string, ICommandConfig> _commands = new();

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
      if ( _commands.ContainsKey( commandNameLower ) )
        throw new InvalidOperationException( $"Command with name \"{commandNameLower}\" already exists." );

      _commands[commandNameLower] = command;
    }

    public void UnregisterCommand( string commandName )
    {
      commandName = commandName.ToLowerInvariant();
      if ( string.IsNullOrWhiteSpace( commandName ) )
        throw new ArgumentException( "Command name cannot be null or whitespace.", nameof( commandName ) );

      if ( !_commands.Remove( commandName ) )
        throw new KeyNotFoundException( $"Command with name \"{commandName}\" does not exist." );
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

    public string[] GetCommandNames() => _commands.Keys.ToArray();

    public bool ExecuteCommand( string commandName, string[] args )
    {
      try
      {
        var command = GetCommand( commandName );
        Log.Info( $"Executing command \"{commandName}\"." );
        return command.CommandFunction( args );
      }
      catch ( Exception e )
      {
        Log.Error( $"Failed to execute command \"{commandName}\": {e.Message}" );
        return false;
      }
    }
  }
}

public static class ConfigManagerHelper
{
  private static readonly Guid ConfigManagerGuid = new Guid( "7c7e467f-11f7-43c7-accf-abc02a3790ea" );

  public static ConfigManager GetConfigManager( Scene scene )
  {
    try
    {
      var configManager = scene.Directory.FindComponentByGuid( ConfigManagerGuid ) as ConfigManager;
      if ( configManager is null )
      {
        Log.Error( "Config Manager not found" );
      }
      return configManager;
    }
    catch ( Exception e )
    {
      Log.Error( $"Failed to get Config Manager: {e.Message}" );
      return null;
    }
  }
}