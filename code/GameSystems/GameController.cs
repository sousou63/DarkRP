using System;
using GameSystems.Jobs;
using GameSystems.Player;
using Sandbox.UI;

namespace GameSystems
{
	public sealed class GameController : Component, Component.INetworkListener
	{
		private static readonly ulong[] DevSteamIDs = new ulong[] {
		    76561198844028104, // Sousou
 		    76561198137204749, // QueenPM
 		    76561198161573319, // irlladdergoat
 		    76561198237485902, // Bozy
 		    76561198040274296, // Stefan
 		    76561198006076880, // dancore
			76561198837197784, // EuroBlue 
			76561199092626415, // Mangro
			76561198243368782,  // Dada
			76561198071517597 // Red
		};
		private static GameController _instance;

		public GameController()
		{
			if ( _instance != null )
			{
				Log.Warning( "Only one instance of GameController is allowed." );
			}
			_instance = this;
		}

		public static GameController Instance => _instance;
		Chat chat { get; set; }
		private Database _database { get; set; } // Don't touch it's waiting when the time will come (when garry releases servers)
		
		[HostSync] public NetDictionary<Guid, PlayerConnObject> Players { get; set; } = new NetDictionary<Guid, PlayerConnObject>();
		[HostSync]
		public NetDictionary<string, UserGroup> UserGroups { get; set; } = new NetDictionary<string, UserGroup>()
		{
			{ "user", new UserGroup( "user", "User", PermissionLevel.User, Color.White ) },
			{ "moderator", new UserGroup( "moderator", "Moderator", PermissionLevel.Moderator, Color.Yellow ) },
			{ "admin", new UserGroup( "admin", "Admin", PermissionLevel.Admin, Color.Red ) },
			{ "superadmin", new UserGroup( "superadmin", "Super Admin", PermissionLevel.SuperAdmin, Color.Blue ) },
			{ "developer", new UserGroup( "developer", "Developer", PermissionLevel.Developer, Color.Orange ) }
		};
		[HostSync]
		public JobSystem JobSystem { get; private set; } = new JobSystem();

		protected override void OnStart()
		{
			chat = Scene.Directory.FindByName( "Screen" )?.First()?.Components.Get<Chat>();
			if ( chat == null ) Log.Error( "Chat component not found" );
      
			if ( !FileSystem.Data.DirectoryExists( "playersdata" ) )
			{
			  FileSystem.Data.CreateDirectory( "playersdata" );
			}

			JobSystem =  new JobSystem();;
		}

		// This could probably be put in the network controller/helper.
		public void AddPlayer( GameObject player, Connection connection )
		{
			Log.Info( $"Adding player: {connection.Id} {connection.DisplayName}" );
			try
			{
				var userGroups = new List<UserGroup>();
				// If the user is a Dev, assign the developer user group
				if ( DevSteamIDs.Contains( connection.SteamId ) )
				{
					userGroups.Add( UserGroups["developer"] );
				}
				// If the user is the host, assign the superadmin user group
				if ( connection.IsHost )
				{
					userGroups.Add( UserGroups["superadmin"] );
				}
				// If the user is not a dev or host, assign the "user" user group
				if ( userGroups.Count == 0 )
				{
					userGroups.Add( UserGroups["user"] );
				}
				Players.Add( connection.Id,new PlayerConnObject( player, connection, userGroups ) );
				if ( Rpc.Caller.IsHost )
				{
					chat?.NewSystemMessage( $"{connection.DisplayName} has joined the game." );
				}
			}
			catch ( Exception e )
			{
				Log.Warning( e );
			}
		}

		public void RemovePlayer( Connection connection )
		{
			Log.Info( $"Removing player: {connection.Id} {connection.DisplayName}" );
			try
			{
				// Find the player in the list
				if ( !Players.TryGetValue( connection.Id, out PlayerConnObject player ) )
				{
					Log.Error( $"Player not found in the list: {connection.Id}" );
					return;
				}

				// Perform clean up functions
				var playerStats = player.GameObject.Components.Get<Stats>();
				if ( playerStats != null )
				{
					playerStats.SellAllDoors();
				}
				
				//Saves player Data
				Log.Info( $"Saving players data: {connection.Id} {connection.DisplayName}" );
				SavedPlayer.SavePlayer( new SavedPlayer(player) );

				// Remove the player from the list
				Players.Remove( connection.Id );
				if ( Rpc.Caller.IsHost )
				{
					chat?.NewSystemMessage( $"{connection.DisplayName} has left the game." );
				}
			}
			catch ( Exception e )
			{
				Log.Warning( e );
			}
		}

		void INetworkListener.OnDisconnected( Connection channel )
		{
			Log.Info( $"Player disconnected: {channel.Id}" );
			RemovePlayer( channel );
		}

		public PlayerConnObject GetPlayerByConnectionID( Guid connection )
		{
			if (Players.TryGetValue(connection, out PlayerConnObject player))
        	{
        	    return player;
        	}
        	return null;
		}

		public PlayerConnObject GetPlayerByGameObjectID( Guid gameObjectID )
		{
			foreach ( var player in Players )
			{
				if ( player.Value.GameObject.Id == gameObjectID )
				{
					return player.Value;
				}
			}
			return null;
		}

		public PlayerConnObject GetPlayerByName( string name )
		{
			foreach ( var player in Players )
			{
				if ( player.Value.Connection.DisplayName.StartsWith(name, StringComparison.OrdinalIgnoreCase) )
				{
					return player.Value;
				}
			}
			return null;
		}

		public PlayerConnObject GetPlayerBySteamID( ulong steamID )
		{
			foreach ( var player in Players )
			{
				if ( player.Value.Connection.SteamId == steamID )
				{
					return player.Value;
				}
			}
			return null;
		}

		/// <summary>
		/// Returns the UserGroup with the specified name.
		/// </summary>
		public UserGroup GetUserGroup( string name )
		{
			if ( UserGroups.TryGetValue( name, out UserGroup userGroup ) )
			{
				return userGroup;
			}
			return null;
		}

		public NetDictionary<Guid, PlayerConnObject> GetAllPlayers()
		{
			return Players;
		}

		/// <summary>
		/// Attempts to find a Player by SteamID first, then by Name.
		/// This should be used for user input,
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public PlayerConnObject PlayerLookup( string input )
		{
			PlayerConnObject foundPlayer = null;
			// Find the player
			// If args[0] can be parsed as ulong, then try to lookup with SteamID first
			if ( ulong.TryParse( input, out var steamID ) )
			{
				foundPlayer = GetPlayerBySteamID( steamID );
			}

			// If not found by SteamID, try to find by name
			foundPlayer ??= GetPlayerByName( input );

			return foundPlayer;
		}
	}

}
