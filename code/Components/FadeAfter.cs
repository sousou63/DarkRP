namespace Scenebox;

public sealed class FadeAfter : Component
{
    [Property] public float Time { get; set; } = 10f;
    [Property] public float FadeTime { get; set; } = 2f;

    ModelRenderer modelRenderer;
    DecalRenderer decalRenderer;

    TimeSince timeSinceStart;

    protected override void OnStart()
    {
        modelRenderer = Components.Get<ModelRenderer>( FindMode.EnabledInSelfAndChildren );
        if ( !modelRenderer.IsValid() )
        {
            decalRenderer = Components.Get<DecalRenderer>( FindMode.EnabledInSelfAndChildren );
        }
        timeSinceStart = 0;
    }

    protected override void OnUpdate()
    {
        if ( timeSinceStart > Time )
        {
            var alpha = MathX.LerpTo( 1, 0, (timeSinceStart - Time) / FadeTime );
            if ( modelRenderer.IsValid() )
            {
                modelRenderer.Tint = modelRenderer.Tint.WithAlpha( alpha );
            }
            else if ( decalRenderer.IsValid() )
            {
                decalRenderer.TintColor = decalRenderer.TintColor.WithAlpha( alpha );
            }
        }

        if ( timeSinceStart > Time + FadeTime )
        {
            GameObject.Destroy();
        }
    }


}