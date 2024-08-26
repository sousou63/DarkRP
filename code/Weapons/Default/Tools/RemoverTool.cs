namespace Scenebox.Tools;

[Tool( "Remover", "Remove GameObjects", "Construction" )]
public class RemoverTool : BaseTool
{
    public override string Attack1Control => "Remove selected object";
    
    public override void PrimaryUseStart()
    {
	    // TODO: Get camera and chagne direction to FOrward
        var tr = Game.ActiveScene.Trace.Ray( new Ray( Toolgun.Player.GameObject.Transform.Position, Toolgun.Player.GameObject.Transform.LocalPosition ), 2000 )
            .WithoutTags( "trigger" )
            .Run();

        if ( !tr.Hit ) return;
        if ( tr.GameObject.Tags.HasAny( "player", "grabbed", "map" ) ) return;

        Toolgun.BroadcastUseEffects( tr.HitPosition, tr.Normal );
        tr.GameObject.Network.TakeOwnership();
        tr.GameObject.Destroy();
    }
}
