using System;
using PlayerDetails;

public sealed class GameController : Component, Component.INetworkListener
{
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

	[HostSync] public NetList<Player> Players { get; set; } = new NetList<Player>();

	protected override void OnStart()
	{
		Log.Info( GameObject.Id );
		chat = Scene.Directory.FindByName( "Screen" )?.First()?.Components.Get<Chat>();
		if ( chat == null ) Log.Error( "Chat component not found" );
	}

	// This could probably be put in the network controller/helper.
	public void AddPlayer( GameObject player, Connection connection )
	{
		Log.Info( $"Adding player: {connection.Id} {connection.DisplayName}" );
		try
		{
			Players.Add( new Player( player, connection ) );
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
		try
		{
			// Find the player in the list
			var playerToRemove = Players.Single( x => x.Connection.Id == connection.Id );

			if ( playerToRemove == null )
			{
				Log.Error( $"Player not found in the list: {connection.Id}" );
				return;
			}

			// Perform clean up functions
			var playerStats = playerToRemove.GameObject.Components.Get<PlayerStats>();
			if ( playerStats != null )
			{
				playerStats.SellAllDoors();
			}

			// Remove the player from the list
			Players.Remove( playerToRemove );
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

	public Player GetPlayerByConnectionID( Guid connection )
	{
		return Players.Single( x => x.Connection.Id == connection );
	}

	public Player GetPlayerByGameObjectID( Guid gameObject )
	{
		return Players.Single( x => x.GameObject.Id == gameObject );
	}

	public Player GetPlayerByName( string name )
	{
		return Players.Single( x => x.Connection.DisplayName.StartsWith( name, StringComparison.OrdinalIgnoreCase ) );
	}

	public Player GetPlayerBySteamID( ulong steamID )
	{
		return Players.Single( x => x.Connection.SteamId == steamID );
	}

	public NetList<Player> GetAllPlayers()
	{
		return Players;
	}

	/// <summary>
	/// Attempts to find a Player by SteamID first, then by Name.
	/// This should be used for user input,
	/// </summary>
	/// <param name="input"></param>
	/// <returns></returns>
	public Player PlayerLookup( string input )
	{
		Player foundPlayer = null;
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
