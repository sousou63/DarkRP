using System.ComponentModel.DataAnnotations;
using GameSystems.Player;

namespace GameSystems;

public class SavedPlayer
{
	public ulong SteamId { get; set; }
	
	// Every parameter should be marked with [Sync] in the original class
	public float Money { get; set; }

	public SavedPlayer() { }
	
	public SavedPlayer(PlayerConnObject player)
	{
		this.SteamId=player.Connection.SteamId;
		this.Money = player.GameObject.Components.Get<Stats>().MoneyBase;
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
