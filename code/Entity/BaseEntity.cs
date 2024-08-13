using Sandbox;
using PlayerDetails;

/// <summary>
/// Represents a generic base entity that provides common functionality 
/// for various types of interactable entities, such as dropped money, printers, food, etc.
/// </summary>
[Library("base_entity", Title = "Base Entity")]
public partial class BaseEntity : Prop, IInteractable
{
    /// <summary>
    /// Gets or sets the health of the entity.
    /// </summary>
    [Net] public float Health { get; set; } = 100f;

    /// <summary>
    /// Gets or sets the name of the entity.
    /// </summary>
    [Net] public string EntityName { get; set; } = "Base Entity";

    /// <summary>
    /// Gets or sets the owner of the entity.
    /// </summary>
    [Net] public Player Owner { get; set; }

    /// <summary>
    /// Called when the entity is spawned in the game world.
    /// </summary>
    public override void Spawn()
    {
        base.Spawn();

        // Set a default model for the entity. This can be overridden in derived classes.
        SetModel("models/citizen_props/crate01.vmdl");
        SetupPhysicsFromModel(PhysicsMotionType.Dynamic);

        Log.Info($"{EntityName} has spawned with {Health} health.");
    }

    /// <summary>
    /// Called when the entity takes damage.
    /// </summary>
    /// <param name="damageInfo">Information about the damage taken.</param>
    public override void TakeDamage(DamageInfo damageInfo)
    {
        base.TakeDamage(damageInfo);

        Health -= damageInfo.Damage;
        Log.Info($"{EntityName} took {damageInfo.Damage} damage. Health is now {Health}.");

        if (Health <= 0)
        {
            OnDestroyed();
        }
    }

    /// <summary>
    /// Called when the entity's health reaches zero and it is destroyed.
    /// </summary>
    protected virtual void OnDestroyed()
    {
        Log.Info($"{EntityName} has been destroyed.");
        Delete();
    }

    /// <summary>
    /// Called when another entity starts touching this entity.
    /// </summary>
    /// <param name="other">The other entity that started touching this entity.</param>
    public override void StartTouch(Entity other)
    {
        base.StartTouch(other);

        if (other is Player player)
        {
            OnPlayerTouch(player);
        }
    }

    /// <summary>
    /// Called when a player touches the entity. Can be overridden to provide specific behavior.
    /// </summary>
    /// <param name="player">The player that touched the entity.</param>
    protected virtual void OnPlayerTouch(Player player)
    {
        Log.Info($"{EntityName} touched by {player.GameObject}");
    }

    /// <summary>
    /// Sets the owner of the entity.
    /// </summary>
    /// <param name="player">The player to set as the owner.</param>
    [Input]
    public void SetOwner(Player player)
    {
        Owner = player;
        Log.Info($"{EntityName} is now owned by {player.GameObject}");
    }

    /// <summary>
    /// Defines the interaction logic when a player uses the default interaction key ("E").
    /// </summary>
    /// <param name="tr">The result of the scene trace.</param>
    /// <param name="player">The player interacting with the entity.</param>
    public virtual void InteractUse(SceneTraceResult tr, Player player)
    {
        Log.Info($"{player.GameObject} used {EntityName} with default interaction.");
    }

    /// <summary>
    /// Defines the interaction logic when a player uses the special interaction key ("R").
    /// </summary>
    /// <param name="tr">The result of the scene trace.</param>
    /// <param name="player">The player interacting with the entity.</param>
    public virtual void InteractSpecial(SceneTraceResult tr, Player player)
    {
        Log.Info($"{player.GameObject} used {EntityName} with special interaction.");
    }

    /// <summary>
    /// Defines the interaction logic when a player uses the Attack 1 interaction key ("Mouse 1").
    /// </summary>
    /// <param name="tr">The result of the scene trace.</param>
    /// <param name="player">The player interacting with the entity.</param>
    public virtual void InteractAttack1(SceneTraceResult tr, Player player)
    {
        Log.Info($"{player.GameObject} used {EntityName} with Attack 1 interaction.");
    }

    /// <summary>
    /// Defines the interaction logic when a player uses the Attack 2 interaction key ("Mouse 2").
    /// </summary>
    /// <param name="tr">The result of the scene trace.</param>
    /// <param name="player">The player interacting with the entity.</param>
    public virtual void InteractAttack2(SceneTraceResult tr, Player player)
    {
        Log.Info($"{player.GameObject} used {EntityName} with Attack 2 interaction.");
    }
}