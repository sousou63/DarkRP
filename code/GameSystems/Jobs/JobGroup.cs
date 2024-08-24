namespace GameSystems.Jobs
{
    [GameResource( "Job Group Definition", "group", "" )]
    public class JobGroup : GameResource
    {
        [Category( "Description" )]
        public string Name { get; set; }
    }

}