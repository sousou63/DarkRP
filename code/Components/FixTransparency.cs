using Sandbox;

namespace Scenebox;

public sealed class FixTransparency : Component
{
	[Property] ModelRenderer modelRenderer { get; set; }

	protected override void OnStart()
	{
		if ( !modelRenderer.IsValid() ) return;
		modelRenderer.SceneObject.Flags.WantsFrameBufferCopy = true;
		modelRenderer.SceneObject.Flags.IsTranslucent = true;
		modelRenderer.SceneObject.Flags.IsOpaque = false;
	}
}
