using GameSystems.Player;

namespace GameSystems.Jobs
{
	public class JobSystem : Component
	{
		// All the jobs
		public Dictionary<string, Job> Jobs { get; private set; } = new();
		// All the job groups
		public Dictionary<string, JobGroup> JobGroups { get; private set; } = new();

		// On Start load all jobs
		public JobSystem()
		{
			Log.Info( "Loading job groups..." );
			foreach ( var group in ResourceLibrary.GetAll<JobGroup>( "data/jobs/groups" ) )
			{
				Log.Info( $"Loading group: {group.Name}" );
				JobGroups[group.Name] = group;
			}

			Log.Info( "Loading jobs..." );
			foreach ( var job in ResourceLibrary.GetAll<Job>("data/jobs"))
			{
				Log.Info( $"Loading job: {job.Name}" );
				Jobs[job.Name] = job;
			}
		}

		// Get default job
		public static Job GetDefault()
		{
			return ResourceLibrary.Get<Job>( "data/jobs/citizen.job" );;
		}
	}
}
