using Sandbox.GameSystems.Player;

namespace Sandbox.GameSystems.Database;

public class SavedPlayer
{
	public ulong SteamId { get; set; }
	
	// Every parameter should be marked with [Sync] in the original class
	public float Money { get; set; }

	public SavedPlayer() { }
	
	public SavedPlayer(NetworkPlayer networkPlayer)
	{
		SteamId = networkPlayer.Connection.SteamId;
		Money = networkPlayer.GameObject.Components.Get<Sandbox.GameSystems.Player.Player>().Balance;
	}

	public static SavedPlayer LoadSavedPlayer(ulong SteamId)
	{
		if ( FileSystem.Data.FileExists( "playersdata/" + SteamId ) )
		{
			return FileSystem.Data.ReadJson<SavedPlayer>( "playersdata/" + SteamId );
		}
		return null;
	}

	public static void SavePlayer( SavedPlayer player )
	{
		FileSystem.Data.WriteJson( "playersdata/" + player.SteamId,player );
	}
}
