[GameResource("Job Resource", "job", "")]
public class JobResource : GameResource
{
	public string Name { get; set; }
	public string Color { get; set;  }
	public bool CanJoin { get; set; }
}

public class JobManager : Component
{
	[HostSync] private static NetList<JobResource> JobResources { get; set; } = new NetList<JobResource>();

	private static int CreateJob( JobResource job )
	{
		JobResources.Add(job);
		
		TeamManager.SetUp(job.Name, job.Color, job.CanJoin);

		return JobResources.Count;
	}

	public static void LoadResources()
	{
		foreach ( var job in ResourceLibrary.GetAll<JobResource>() )
		{
			CreateJob( job );
		}
	}
}
