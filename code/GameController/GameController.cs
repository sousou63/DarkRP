using Sandbox;

public sealed class GameController : Component, Component.INetworkListener
{

	public class PlayerConnection
	{
		public GameObject GameObject { get; set; }
		public Connection Connection { get; set; }

		public PlayerConnection( GameObject gameObject, Connection connection )
		{
			GameObject = gameObject;
			Connection = connection;
		}
	}
	public List<PlayerConnection> Players { get; set; } = new List<PlayerConnection>();


	public void AddPlayer( GameObject player, Connection connection )
	{
		Players.Add( new PlayerConnection( player, connection ) );
	}

	public void RemovePlayer( Connection connection )
	{
		// Find players to be removed
		var playersToRemove = Players.Where( x => x.Connection.Id == connection.Id ).ToList();

		// Run clean-up function on each player
		foreach ( var player in playersToRemove )
		{
			// Clean up process
			var playerStats = player.GameObject.Components.Get<PlayerStats>();
			if ( playerStats == null ) return;
			playerStats.SellAllDoors();

		}

		// Remove players from the list
		Players.RemoveAll( x => x.Connection.Id == connection.Id );
	}

	void INetworkListener.OnDisconnected( Connection channel )
	{
		RemovePlayer( channel );
	}
}
