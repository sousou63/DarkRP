
namespace GameSystems.Jobs
{
    [GameResource( "Job Group Definition", "jobgroup", "" )]
    public class JobGroup : GameResource
    {
        [Category( "Info" )]
        public string Name { get; set; }
    }

}