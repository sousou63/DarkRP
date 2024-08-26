using GameSystems.Player;

namespace GameSystems.Jobs
{
	public static class JobProvider
	{
		public static Dictionary<string, JobResource> Jobs { get; private set; } = new();
		public static Dictionary<string, JobGroupResource> JobGroups { get; private set; } = new();

		// On Start load all jobs
		static JobProvider()
		{
			Log.Info( "Loading groups..." );
			// Get all JobGroup resources
			foreach ( var group in ResourceLibrary.GetAll<JobGroupResource>( "data/jobs/groups" ) )
			{
				Log.Info( $"Loading group: {group.Name}" );
				JobGroups[group.Name] = group;
			}

			Log.Info( "Loading jobs..." );
			// Get all Job resources
			foreach ( var job in ResourceLibrary.GetAll<JobResource>( "data/jobs" ) )
			{
				Log.Info( $"Loading job: {job.Name}" );
				Jobs[job.Name] = job;
			}
		}

		// Get default job when player spawns
		public static JobResource GetDefault()
		{
			return ResourceLibrary.Get<JobResource>( "data/jobs/citizen.job" ); ;
		}
	}
}
