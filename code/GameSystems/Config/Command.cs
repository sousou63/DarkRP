using System;
using GameSystems.Player;

namespace GameSystems.Config {
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
		public PermissionLevel PermissionLevel { get; } = PermissionLevel.User;

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
		public Command( string name, string description, PermissionLevel permissionLevel, Func<GameObject, Scene, string[], bool> commandFunction )
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
}