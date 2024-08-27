
using System;

namespace Scenebox;

public class MeleeWeapon : Weapon
{
    [Property] public float Cooldown { get; set; } = 1f;
    [Property] public float Range { get; set; } = 100f;

    TimeSince timeSinceLastAttack = 10f;

    public override void Update()
    {
        if ( !IsEquipped ) return;

        if ( Input.Down( "Attack1" ) )
        {
            TryAttack();
        }
    }

    void TryAttack()
    {
        if ( timeSinceLastAttack < Cooldown ) return;

        var tr = Scene.Trace.Ray( new Ray( Player.Head.Transform.Position, Player.Direction.Forward ), Range )
            .IgnoreGameObjectHierarchy( GameObject.Root )
            .WithoutTags( "trigger" )
            .Radius( 1f )
            .Run();

        Attack( tr );
        BroadcastAttackAnimation();

        timeSinceLastAttack = 0f;
    }

    [Broadcast]
    void BroadcastAttackAnimation()
    {
        var playerRenderer = Player?.Body?.Components?.Get<SkinnedModelRenderer>();
        playerRenderer?.Set( "b_attack", true );
        Player?.ViewModel?.ModelRenderer?.Set( "b_attack", true );
    }
}