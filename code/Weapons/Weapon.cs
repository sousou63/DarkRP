
using System;
using Entity.Interactable.Props;
using GameSystems.Player;
using Sandbox.Citizen;
using Sandbox.GameResources;
using Sandbox.GameSystems.Player;

namespace Scenebox;

public class Weapon : Component
{
    [Property] public WeaponResource Resource { get; set; }

    [Property] public SkinnedModelRenderer ModelRenderer { get; set; }
    [Property] protected CitizenAnimationHelper.HoldTypes HoldType { get; set; } = CitizenAnimationHelper.HoldTypes.Pistol;

    [Property] public float Damage { get; set; } = 20f;
    [Property] protected float Force { get; set; } = 20f;


    [Property, Group("Sounds")] public SoundEvent EquipSound { get; set; }

    [Property, Group("References")] public GameObject Muzzle { get; set; }

    public Sandbox.GameSystems.Player.Player Player => Components.Get<Sandbox.GameSystems.Player.Player>(FindMode.EverythingInAncestors);

    [Sync]
    public bool IsEquipped
    {
        get => _isEquipped;
        set
        {
            _isEquipped = value;

            if (ModelRenderer.IsValid())
                ModelRenderer.Enabled = _isEquipped;
        }
    }
    bool _isEquipped;

    public int Ammo { get; set; } = 0;
    public int AmmoReserve { get; set; } = 0;


    protected override void OnStart()
    {
        if (IsEquipped)
            OnEquip();
        else
            OnUnequip();
    }

    public virtual void Update() { }
    public virtual void FixedUpdate() { }

    [Authority]
    public void Equip()
    {
        if (IsEquipped) return;

        // if (Player.IsValid())
        // {
        //     var weapons = Player.Inventory.Weapons.ToList();
        //
        //     foreach (var weapon in weapons)
        //     {
        //         weapon.Unequip();
        //     }
        // }

        IsEquipped = true;
        GameObject.Enabled = true;

        OnEquip();
    }

    [Authority]
    public void Unequip()
    {
        if (!IsEquipped) return;

        IsEquipped = false;
        GameObject.Enabled = false;

        OnUnequip();
    }
    

    protected void UpdateRenderMode()
    {
        foreach (var renderer in Components.GetAll<ModelRenderer>())
        {
            renderer.RenderType = IsProxy ? Sandbox.ModelRenderer.ShadowRenderType.On : Sandbox.ModelRenderer.ShadowRenderType.ShadowsOnly;
        }
    }

    protected virtual void OnEquip()
    {
        BroadcastSetVisible(true);
    }

    protected virtual void OnUnequip()
    {
        BroadcastSetVisible(false);

    }

    protected override void OnDestroy()
    {
    }

    protected virtual void Attack(SceneTraceResult tr)
    {
        if (tr.Hit)
        {
            Sound.Play(tr.Surface.Sounds.ImpactHard, tr.HitPosition);
            string decal = "";
            var decals = tr.Surface.ImpactEffects.BulletDecal;
            if ((decals?.Count() ?? 0) > 0)
                decal = decals.MinBy(x => Random.Shared.Float());

	        // TODO: Player takes damage
            // if (tr.GameObject?.Root?.Components?.TryGet<PlayerConnObject>(out var player) ?? false)
            // {
            //     player(Damage, Resource.ResourceId);
            // }

            // TODO Decels
            // GameManager.Instance.SpawnDecal(decal, tr.HitPosition, tr.Normal, tr.GameObject?.Id ?? Guid.Empty);
        }
    }

    [Broadcast]
    void BroadcastSetVisible(bool visible)
    {
        if (ModelRenderer.IsValid()) ModelRenderer.Enabled = visible;
    }
}
