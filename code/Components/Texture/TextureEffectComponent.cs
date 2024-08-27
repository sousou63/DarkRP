namespace Scenebox;

public abstract class TextureEffectComponent : Component
{
	public abstract Texture Apply( Texture texture );

	protected Texture CreateTexture( Vector2 targetSize, string name = "TextureEffect" )
	{
		return Texture.Create( (int)targetSize.x, (int)targetSize.y, ImageFormat.RGBA8888 )
										.WithName( name )
										.WithDynamicUsage()
										.WithUAVBinding()
										.Finish();
	}
}