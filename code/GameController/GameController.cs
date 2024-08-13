using System;
using Commands;
using Sandbox;
using PlayerDetails;

public sealed class GameController : Component, Component.INetworkListener
{
    private static GameController _instance;

	public GameController()
	{
			if (_instance != null)
			{
				Log.Warning("Only one instance of GameController is allowed.");
			}
			_instance = this;
	}

	public class Player
	{
		public GameObject GameObject { get; set; }
		public Connection Connection { get; set; }

    public Player(GameObject gameObject, Connection connection)
        {
            GameObject = gameObject;
            Connection = connection;
        }
    }

    public List<Player> Players { get; set; } = new List<Player>();

	// This could probably be put in the network controller/helper.
    public void AddPlayer(GameObject player, Connection connection)
    {
        Log.Info($"Player connected: {connection.Id}");
        Players.Add(new Player(player, connection));
        Log.Info($"Players ({Players.Count}):");
        foreach (var pp in Players)
        {
            Log.Info($"{pp.Connection.DisplayName} ({pp.Connection.Id})");
        }
    }

    public void RemovePlayer(Connection connection)
    {	
		// Find players to be removed
        var playersToRemove = Players.Where(x => x.Connection.Id == connection.Id).ToList();

		// Run clean-up function on each player
        foreach (var player in playersToRemove)
        {
            var playerStats = player.GameObject.Components.Get<PlayerStats>();
            if (playerStats == null) continue; // Use continue instead of return to ensure all players are cleaned up
            playerStats.SellAllDoors();
        }

		// Remove players from the list
        Players.RemoveAll(x => x.Connection.Id == connection.Id);
    }

    void INetworkListener.OnDisconnected(Connection channel)
    {
        Log.Info($"Player disconnected: {channel.Id}");
        RemovePlayer(channel);
    }

    public Player GetPlayerByConnectionID(Guid connection)
    {
        return Players.Find(x => x.Connection.Id == connection);
    }

    public Player GetPlayerByGameObjectID(Guid gameObject)
    {
        return Players.Find(x => x.GameObject.Id == gameObject);
    }
}