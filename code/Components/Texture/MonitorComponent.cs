namespace Scenebox;

public sealed class MonitorComponent : DynamicTextureComponent
{
    [Property] public ModelRenderer Model { get; set; }
    [Property] public Material ScreenMaterial { get; set; }
    [Property] public string AttributeName { get; set; } = "screen";
    [Property] public string MaterialName { get; set; }

    private Dictionary<string, int> _materialIndices = new();

    public override Texture OutputTexture
    {
        get => base.OutputTexture;
        protected set
        {
            base.OutputTexture = value;
            ApplyMaterialOverride();
        }
    }

    private Material _screenMaterial;

    protected override void OnStart()
    {
        Model ??= GameObject.Components.Get<ModelRenderer>();
    }

    public override void OnPostEffect()
    {
        if ( _screenMaterial is null )
        {
            ApplyMaterialOverride();
        }
    }

    public void ApplyMaterialOverride()
    {
        if ( !Game.IsPlaying || Model?.SceneObject is null )
            return;

        if ( !string.IsNullOrWhiteSpace( MaterialName ) )
        {
            UpdateMaterialIndices();
            var originalMaterial = Model.Model.Materials.FirstOrDefault( m => m.ResourceName == MaterialName );
            if ( originalMaterial is null ) return;
            var attribute = $"materialIndex{_materialIndices[MaterialName]}";
            _screenMaterial = ScreenMaterial.CreateCopy();
            Model.SceneObject.SetMaterialOverride( _screenMaterial, attribute );
            _screenMaterial.Set( "Color", OutputTexture );
            _screenMaterial.Set( "SelfIllumMask", OutputTexture );
            return;
        }

        _screenMaterial = ScreenMaterial.CreateCopy();
        _screenMaterial.Set( "Color", OutputTexture );
        _screenMaterial.Set( "SelfIllumMask", OutputTexture );

        if ( string.IsNullOrWhiteSpace( AttributeName ) )
        {
            Model.MaterialOverride = _screenMaterial;
        }
        else
        {
            Model.SceneObject.SetMaterialOverride( _screenMaterial, AttributeName );
        }
    }

    private void UpdateMaterialIndices()
    {
        if ( Model?.SceneObject is null ) return;

        _materialIndices.Clear();
        var materials = Model.Model.Materials.ToArray();
        for ( int i = 0; i < materials.Count(); i++ )
        {
            materials[i].Attributes.Set( "materialIndex" + i, 1 );
            _materialIndices[materials[i].ResourceName] = i;
        }
    }
}