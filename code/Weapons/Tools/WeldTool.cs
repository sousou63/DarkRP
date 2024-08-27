using System;

namespace Scenebox.Tools;

[Tool( "Weld", "Weld stuff together", "Constraints" )]
[Description( "A weld created a fixed constraint between objects." )]
public class WeldTool : BaseTool
{
    [Property, Range( 0, 50000, 1 )]
    [Title( "Force Limit" ), Description( "The constraint will break if the amount of force is greater than this. Set to 0 to not break. Unfortunately due to the way the physgun works, moving objects with a value greater than 0 will instantly break the joint." )]
    public float ForceLimit { get; set; } = 0;

    [Property, Title( "No Collide" )]
    public bool NoCollide { get; set; } = false;

    public override string Attack1Control => SelectedObject.IsValid() ? "Attach the object with a Weld constraint" : "Select an object to begin a Weld constraint";

    GameObject SelectedObject = null;
    int SelectedBodyIndex = 0;

    public override void OnEquip()
    {
        base.OnEquip();

        SelectedObject = null;
    }

    public override void PrimaryUseStart()
    {
        var tr = Game.ActiveScene.Trace.Ray( new Ray( Toolgun.Player.Head.Transform.Position, Toolgun.Player.Direction.Forward ), 2000 )
            .WithoutTags( "trigger" )
            .Run();

        if ( !tr.Hit ) return;
        if ( tr.GameObject.Tags.HasAny( "player", "grabbed", "map" ) ) return;
        if ( !tr.Body.IsValid() ) return;

        Toolgun.BroadcastUseEffects( tr.HitPosition, tr.Normal );

        if ( SelectedObject.IsValid() )
        {
            CompleteWeld( tr.Body, tr.GameObject );
            SelectedObject = null;
            return;
        }

        SelectedObject = tr.GameObject;
        SelectedBodyIndex = tr.Body.GroupIndex;
    }

    void CompleteWeld( PhysicsBody body, GameObject obj )
    {
        if ( SelectedObject.Tags.Has( "grabbed" ) || obj.Tags.Has( "grabbed" ) ) return;

        SelectedObject.Network.TakeOwnership();

        if ( SelectedBodyIndex >= 0 )
        {
            var renderer = SelectedObject.Root.Components.Get<SkinnedModelRenderer>();
            if ( renderer.IsValid() )
            {
                renderer.CreateBoneObjects = true;
                SelectedObject = renderer.GetBoneObject( SelectedBodyIndex );
            }
        }

        if ( body.GroupIndex >= 0 )
        {
            obj.Network.TakeOwnership();
            var renderer = obj.Root.Components.Get<SkinnedModelRenderer>();
            if ( renderer.IsValid() )
            {
                renderer.CreateBoneObjects = true;
                obj.Network.Refresh();
                obj = renderer.GetBoneObject( body.GroupIndex );
            }
        }

        FixedJoint weld = SelectedObject.Components.GetAll<FixedJoint>().FirstOrDefault( x => x.Body == obj );
        if ( !weld.IsValid() )
        {
            weld = SelectedObject.Components.Create<FixedJoint>();
        }
        weld.Body = obj;
        weld.BreakForce = ForceLimit;
        weld.EnableCollision = !NoCollide;
        var rootObject = SelectedObject.Root;
        rootObject.Network.Refresh();

        Log.Info( $"{ForceLimit} {NoCollide} {SelectedObject.Id} {obj.Id}" );

        UndoManager.Instance.Add( "Undone Weld", new List<Guid>() { SelectedObject.Id, obj.Id }, () =>
        {
            if ( weld.IsValid() )
            {
                weld.Destroy();
                rootObject.Network.Refresh();
            }
        } );
    }
}