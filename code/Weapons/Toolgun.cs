using Scenebox.Tools;
using Scenebox.UI;

namespace Scenebox;

public class Toolgun : Weapon
{

    [Property, Group( "Sounds" )] SoundEvent UseSound { get; set; }

    [Property, Group( "Prefabs" )] GameObject LinePrefab { get; set; }
    [Property, Group( "Prefabs" )] GameObject MarkerPrefab { get; set; }

    internal BaseTool CurrentTool = null;

    protected override void OnStart()
    {
        if ( CurrentTool == null )
            SetTool( TypeLibrary.GetType<BaseTool>( "Scenebox.Tools.RemoverTool" ) );
    }

    public override void Update()
    {
        if ( !IsEquipped ) return;

        if ( Input.Pressed( "attack1" ) ) CurrentTool?.PrimaryUseStart();
        if ( Input.Down( "attack1" ) ) CurrentTool?.PrimaryUseUpdate();
        if ( Input.Released( "attack1" ) ) CurrentTool?.PrimaryUseEnd();

        if ( Input.Pressed( "attack2" ) ) CurrentTool?.SecondaryUseStart();
        if ( Input.Down( "attack2" ) ) CurrentTool?.SecondaryUseUpdate();
        if ( Input.Released( "attack2" ) ) CurrentTool?.SecondaryUseEnd();
    }

    public void SetTool( TypeDescription toolDescription )
    {
        if ( CurrentTool != null )
        {
            if ( CurrentTool.GetType() == toolDescription.TargetType ) return;

            CurrentTool?.OnUnequip();
            CurrentTool = null;
        }

        if ( toolDescription == null ) return;

        CurrentTool = TypeLibrary.Create<BaseTool>( toolDescription.TargetType );
        CurrentTool.Toolgun = this;
        CurrentTool?.OnEquip();

        ToolMenu.Instance?.UpdateInspector();
    }

    protected override void OnEquip()
    {
        base.OnEquip();
        CurrentTool?.OnEquip();
    }

    [Broadcast]
    public void BroadcastUseEffects( Vector3 hitPosition, Vector3 hitNormal = default )
    {
        var startPosition = (Player?.ViewModel?.Muzzle ?? Muzzle).Transform.Position;

        var playerRenderer = Player?.Body?.Components?.Get<SkinnedModelRenderer>();
        playerRenderer?.Set( "b_attack", true );
        Player?.ViewModel?.ModelRenderer?.Set( "b_attack", true );

        MarkerPrefab.Clone( hitPosition, Rotation.LookAt( hitNormal, Vector3.Up ) );
        var lineObj = LinePrefab.Clone( startPosition );
        lineObj.BreakFromPrefab();
        var line = lineObj.Components.Get<LineParticle>( FindMode.EverythingInSelfAndDescendants );
        line.Init( startPosition, hitPosition );

        var sound = Sound.Play( UseSound, startPosition );
        if ( Connection.Local.Id == Rpc.CallerId ) sound.ListenLocal = true;
    }

}