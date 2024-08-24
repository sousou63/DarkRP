using GameSystems.Player;

namespace GameSystems.Jobs
{
	public class JobSystem : Component
	{
		public Dictionary<string,Job> Jobs { get; private set; } = new Dictionary<string, Job>();

		// On Start load all jobs
		public JobSystem()
		{
			Log.Info( "Loading jobs" );
			foreach ( var job in ResourceLibrary.GetAll<Job>("data/jobs"))
			{
				Log.Info( $"Loading job: {job.Name}" );
				Jobs[job.Name] = job;
			}
		}
		public static Job GetDefault()
		{
			return ResourceLibrary.Get<Job>( "data/jobs/citizen.job" );;
		}
	}
}
