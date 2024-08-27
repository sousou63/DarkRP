namespace Scenebox;

public sealed class WeaponPickup : Component, Component.ICollisionListener
{
    public WeaponResource WeaponResource { get; set; }

    TimeSince timeSinceSpawn = 0;

    protected override void OnStart()
    {
        timeSinceSpawn = 0;

        var renderers = Components.GetAll<Renderer>( FindMode.DisabledInSelfAndDescendants );
        foreach ( var renderer in renderers )
        {
            renderer.Enabled = true;
        }
    }

    public void OnCollisionStart( Collision collision )
    {
        if ( WeaponResource is null ) return;
        if ( timeSinceSpawn < 1f ) return;

        if ( collision.Other.GameObject.Root.Components.TryGet<Player>( out var player ) )
        {
            player.Inventory.GiveWeapon( WeaponResource );
            GameManager.Instance.BroadcastDestroyObject( GameObject.Id );
        }
    }
}