using Sandbox;
using Sandbox.UI;

namespace Scenebox;

public sealed class TexturePanel : PanelComponent
{
	[Property] public DynamicTextureComponent TextureTarget { get; set; }
	[Property, Range( 0, 90, 1f )] public float MarginPercent { get; set; } = 0f;
	[Property]
	public bool UseScreenBounds
	{
		get => _useScreenBounds;
		set
		{
			_useScreenBounds = value;
			if ( !_useScreenBounds && _rootPanel.IsValid() )
			{
				_rootPanel.PanelBounds = CustomPanelBounds;
			}
		}
	}
	private bool _useScreenBounds;
	[Property] public Rect CustomPanelBounds { get; set; } = new Rect( 0, 0, 1000, 1000 );
	[Property] public Rect CurrentPanelBounds => _rootPanel?.PanelBounds ?? new Rect();
	[Property] public Vector2 CurrentScreenSize => Screen.Size;
	[Property] public float CurrentPanelScale => _rootPanel?.Scale ?? 0f;
	[Property] public float CurrentPanelScaleFromScreen => _rootPanel?.ScaleFromScreen ?? 0f;
	[Property] public float CurrentPanelScaleToScreen => _rootPanel?.ScaleToScreen ?? 0f;

	private SceneCustomObject _renderObject;
	private RootPanel _rootPanel;
	private Texture _texture;

	protected override void OnStart()
	{
		EnsureRootPanel();
		_renderObject = new SceneCustomObject( Scene.SceneWorld );
		_renderObject.RenderOverride = Render;
		if ( _rootPanel is not null )
		{
			CreateInputTexture();
		}
	}

	protected override void OnEnabled()
	{
		base.OnEnabled();
		EnsureRootPanel();
		Panel.Parent = _rootPanel;
		if ( !UseScreenBounds )
		{
			_rootPanel.PanelBounds = CustomPanelBounds;
		}
	}

	protected override void OnDisabled()
	{
		Panel?.Delete();
		_rootPanel?.Delete();
		_rootPanel = null;
	}

	protected override void OnUpdate()
	{
		if ( UseScreenBounds )
		{
			UpdateScreenBounds();
		}
		else
		{
			//_rootPanel.PanelBounds = CustomPanelBounds;
			Panel.Style.Width = Length.Percent( 100f - MarginPercent );
			Panel.Style.Left = Length.Percent( MarginPercent / 2f );
			Panel.Style.Height = Length.Percent( 100f - MarginPercent );
			Panel.Style.Top = Length.Percent( MarginPercent / 2f );
		}
	}

	private void UpdateScreenBounds()
	{
		_rootPanel.PanelBounds = new Rect( 0, 0, Screen.Width, Screen.Height );
		var aspect = Screen.Width / Screen.Height;
		// For my own sake, I assume that your screen is not vertical.
		Panel.Style.Width = Length.Percent( 100f - MarginPercent );
		Panel.Style.Left = Length.Percent( MarginPercent / 2f );
		Panel.Style.Height = Length.Percent( Panel.Style.Width.Value.Value / aspect );
		Panel.Style.Top = Length.Percent( Panel.Style.Height.Value.Value / 2f + MarginPercent / 2f );
	}

	private void EnsureRootPanel()
	{
		if ( _rootPanel is not null )
			return;

		_rootPanel = new RootPanel
		{
			RenderedManually = true,
			PanelBounds = new Rect( 0, 0, 1000, 1000 )
		};
		_rootPanel.Style.Position = PositionMode.Absolute;
		_rootPanel.Style.Width = Length.Pixels( 1920 );
		_rootPanel.Style.Height = Length.Percent( 1080 );
		_rootPanel.Style.BackgroundColor = Color.Transparent;
		_rootPanel.StateHasChanged();
	}

	private void CreateInputTexture()
	{
		_texture?.Dispose();
		_texture = Texture.CreateRenderTarget()
				 .WithSize( _rootPanel.PanelBounds.Size )
				 //  .WithScreenFormat()
				 //  .WithScreenMultiSample()
				 .Create();
		TextureTarget.SetTexture( _texture );
	}

	private void Render( SceneObject sceneObject )
	{
		EnsureRootPanel();

		if ( _rootPanel is null )
			return;

		if ( TextureTarget.InputTexture is null || TextureTarget.InputTexture.Size != _rootPanel.PanelBounds.Size )
		{
			CreateInputTexture();
			return;
		}
		Graphics.RenderTarget = RenderTarget.From( TextureTarget.InputTexture );
		Graphics.Attributes.SetCombo( "D_WORLDPANEL", 0 );
		Graphics.Viewport = new Rect( 0, _rootPanel.PanelBounds.Size );
		Graphics.Clear( Color.Transparent );

		_rootPanel.RenderManual( 1f );

		Graphics.RenderTarget = null;
	}
}