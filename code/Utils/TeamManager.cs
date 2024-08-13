
public class Team
{
	public string Name { get; set; }
	public string Color { get; set; }
	public bool CanJoin { get; set; }
	
	public Team(string name, string color, bool canJoin)
	{
		Name = name;
		Color = color;
		CanJoin = canJoin;
	}
}

public abstract class TeamManager : Component
{
	[Sync] private static NetDictionary<int, Team> Teams { get; } = new NetDictionary<int, Team>();
	private const string DefaultColor = "#FFFFF";
	private static int _nextTeamId = 0;

	public static void InitializeTeams()
	{
		ClearTeams();
		
		Teams[0] = new Team("Unassigned", DefaultColor, false);
		_nextTeamId = 1;
	}
	
	public static void SetUp( string name, string color, bool canJoin = true )
	{
		var id = _nextTeamId++;
		Teams[id] = new Team( name, color, canJoin );
	}

	public static Team GetTeam( int id )
	{
		if ( !Teams.ContainsKey( id ) )
		{
			Log.Warning($"There is no Team with Index: {id}");
		}

		return Teams[id];
	}

	public static void LogTeams()
	{
		Log.Info($"All Current Teams:");
		foreach ( var team in Teams )
		{
			Log.Info($"Team Name: {team.Value.Name}, Team ID: {team.Key}");
		}
	}

	private static void ClearTeams()
	{
		Teams.Clear();
		_nextTeamId = 0;
	}

	[ConCmd("PrintTeams")]
	public static void PrintTeams()
	{
		Log.Info($"All Current Teams:");
		foreach ( var team in Teams )
		{
			Log.Info($"Team Name: {team.Value.Name}, Team ID: {team.Key}");
		}
	}
}
