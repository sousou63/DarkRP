public static class ConnectionExtensions
{

	[ConVar( "dev_admin" )]
	public static bool DevsAreAdmins { get; set; } = true;

	public const ulong DEV_STEAM_ID = 0;

	public static bool IsAdmin( this Connection conn )
		=> conn?.IsHost == true || ( DevsAreAdmins && IsDev( conn ) );

	public static bool IsDev( this Connection conn )
		=> conn?.SteamId == DEV_STEAM_ID;

	public static string GetLogName( this Connection conn )
	{
		if ( conn is null )
			return "(System)";

		return $"({conn.SteamId}\\{conn.DisplayName})";
	}
}
