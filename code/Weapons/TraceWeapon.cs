
using System;

namespace Scenebox;

public class TraceWeapon : Weapon
{
    [Property] public float Cooldown { get; set; } = 0.3f;
    [Property] public float Range { get; set; } = 2000f;
    [Property] public int BulletCount { get; set; } = 1;
    [Property] public float Spread { get; set; } = 0;
    [Property] public float ReloadTime { get; set; } = 1;

    [Property, Group( "Sounds" )] SoundEvent ShootSound { get; set; }

    TimeSince timeSinceLastAttack = 10f;

    bool reloading = false;
    TimeSince timeSinceLastReload = 10f;

    protected override void OnStart()
    {
        Ammo = Resource.ClipSize;
        AmmoReserve = Resource.StartingReserve;
    }

    public override void Update()
    {
        if ( !IsEquipped ) return;

        if ( reloading && timeSinceLastReload >= ReloadTime )
        {
            int existingAmmo = Ammo;
            reloading = false;
            Ammo = Math.Min( Resource.ClipSize, AmmoReserve );
            AmmoReserve -= Ammo - existingAmmo;
        }

        if ( Input.Pressed( "reload" ) )
        {
            Reload();
        }

        if ( Input.Down( "Attack1" ) )
        {
            TryAttack();
        }
    }

    void TryAttack()
    {
        if ( timeSinceLastAttack < Cooldown ) return;
        if ( reloading ) return;

        if ( Ammo <= 0 )
        {
            Reload();
            return;
        }

        for ( int i = 0; i < BulletCount; i++ )
        {
            var angles = Player.Direction + new Angles( Random.Shared.Float( -Spread, Spread ), Random.Shared.Float( -Spread, Spread ), 0 );
            var tr = Scene.Trace.Ray( new Ray( Player.Head.Transform.Position, angles.Forward ), Range )
                .IgnoreGameObjectHierarchy( GameObject.Root )
                .WithoutTags( "trigger" )
                .Run();

            Attack( tr );
            BroadcastBulletTrail( tr.StartPosition, tr.EndPosition, tr.Distance, 0 );
        }

        Ammo--;
        BroadcastAttackAnimation();
        timeSinceLastAttack = 0f;
    }

    void Reload()
    {
        if ( reloading ) return;
        if ( AmmoReserve <= 0 ) return;

        reloading = true;
        timeSinceLastReload = 0f;

        BroadcastReload();
    }

    [Broadcast]
    void BroadcastAttackAnimation()
    {
        var playerRenderer = Player?.Body?.Components?.Get<SkinnedModelRenderer>();
        playerRenderer?.Set( "b_attack", true );
        Player?.ViewModel?.ModelRenderer?.Set( "b_attack", true );
        if ( ShootSound is not null )
        {
            var sound = Sound.Play( ShootSound, Transform.Position );
            if ( Connection.Local.Id == Rpc.CallerId )
            {
                sound.Volume = 0.25f;
                sound.ListenLocal = true;
            }
        }
    }

    [Broadcast]
    void BroadcastReload()
    {
        var playerRenderer = Player?.Body?.Components?.Get<SkinnedModelRenderer>();
        playerRenderer?.Set( "b_reload", true );
        Player?.ViewModel?.ModelRenderer?.Set( "b_reload", true );
    }
}