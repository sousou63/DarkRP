using Sandbox.Audio;

namespace Scenebox;

public partial class PlayerVoiceComponent : Voice
{
    public bool IsMuted { get; set; } = false;

    protected override IEnumerable<Connection> ExcludeFilter()
    {
        if ( IsMuted )
        {
            return new List<Connection>()
            {
                Connection.Local
            };
        }
        return base.ExcludeFilter();
    }
}