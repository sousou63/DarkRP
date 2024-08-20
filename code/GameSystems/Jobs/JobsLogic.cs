using GameSystems.Player;

public class JobsLogic : Component
{
    private static List<Job> Jobs { get; set; } = new();

    // On Start load all jobs
    protected override void OnStart()
    {
        // var jobsConfig = new JobsConfig();
        Jobs.Clear();
        foreach ( var job in JobsConfig.Jobs )
        {
            Jobs.Add( job );
        }
    }

    // Returns a list of all jobs
    public static List<Job> GetJobs()
    {
        Log.Info( $"All Jobs: {Jobs.Count}" );

        return Jobs;
    }

    // Returns a specific job
    public static Job GetJob( int index )
    {
        return Jobs[index];
    }

    // Sets a specific job
    public static void SelectJob( Job job, GameObject player )
    {
        Log.Info( $"Selected Job: {job.Name}" );
        player.Components.Get<Stats>().Job = job;
        var playerStats = player.Components.Get<Stats>();
        // Log salary
        Log.Info( $"Salary: {playerStats.Job.Salary}" );

    }
}