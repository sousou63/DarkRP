namespace Scenebox;

public sealed class DestroyAfter : Component
{
    [Property] public float Time { get; set; } = 10f;

    TimeSince timeSinceStart;

    protected override void OnStart()
    {
        timeSinceStart = 0;
    }

    protected override void OnFixedUpdate()
    {
        if ( timeSinceStart > Time )
        {
            GameObject.Destroy();
        }
    }


}