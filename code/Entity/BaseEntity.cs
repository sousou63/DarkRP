using Sandbox;
using PlayerDetails;

/// <summary>
/// Represents a generic base component that provides common functionality 
/// for various types of interactable entities such as dropped money, printers, food, etc.
/// </summary>
[Library("base_entity", Title = "Base Entity")]
public partial class BaseEntity : Component, Component.IDamageable, Component.ICollisionListener, IInteractable
{
    /// <summary>
    /// Gets or sets the health of the entity.
    /// </summary>
    [Sync] public float Health { get; set; } = 100f;

    /// <summary>
    /// Gets or sets the name of the entity.
    /// </summary>
    [Sync] public string EntityName { get; set; } = "Base Entity";

    /// <summary>
    /// Gets or sets the owner of the entity.
    /// </summary>
    [Sync] public Player Owner { get; set; }

    /// <summary>
    /// Called when the component is first created. Initializes the component and sets default values.
    /// </summary>
    protected override void OnAwake()
    {
        base.OnAwake();
        Log.Info($"{EntityName} has been initialized with {Health} health.");
    }

    /// <summary>
    /// Called when the component is enabled for the first time. 
    /// Used to set up the model and any necessary physics.
    /// </summary>
    protected override void OnStart()
    {
        base.OnStart();

        if (GameObject != null)
        {
            // Create or retrieve a ModelRenderer component and set the model.
            var modelRenderer = GameObject.Components.GetOrCreate<ModelRenderer>();
            modelRenderer.Model = Model.Load("models/citizen_props/crate01.vmdl");
            // Set up physics, if necessary.
        }
    }

    /// <summary>
    /// Called every frame. Handles per-frame updates such as player input or other dynamic behavior.
    /// </summary>
    protected override void OnUpdate()
    {
        base.OnUpdate();
        HandlePlayerInput(); // Placeholder for actual functionality
    }

    /// <summary>
    /// Called at a fixed timestep, typically used for physics updates.
    /// </summary>
    protected override void OnFixedUpdate()
    {
        base.OnFixedUpdate();
        ApplyPhysicsMovement(); // Placeholder for actual functionality
    }

    /// <summary>
    /// Called when the component takes damage. Adjusts the health and checks if the entity should be destroyed.
    /// </summary>
    /// <param name="damageInfo">The damage information.</param>
    public void OnDamage(in DamageInfo damageInfo)
    {
        Health -= damageInfo.Damage;
        Log.Info($"{EntityName} took {damageInfo.Damage} damage. Health is now {Health}.");

        if (Health <= 0)
        {
            OnDestroyed();
        }
    }

    /// <summary>
    /// Called when the entity's health reaches zero. Disables the GameObject or removes the component.
    /// </summary>
    protected void OnDestroyed()
    {
        Log.Info($"{EntityName} has been destroyed.");
        GameObject.Enabled = false; // Disable the GameObject
        // Alternatively, remove the component
        // GameObject.Components.Remove(this);
    }

    /// <summary>
    /// Called when a collision starts with another collider.
    /// </summary>
    /// <param name="other">The collision data.</param>
    public void OnCollisionStart(Collision other)
    {
        Log.Info($"{EntityName} started colliding with {other.Other.GameObject}");
    }

    /// <summary>
    /// Called each physics step while a collision is ongoing.
    /// </summary>
    /// <param name="other">The collision data.</param>
    public void OnCollisionUpdate(Collision other)
    {
        Log.Info($"{EntityName} is colliding with {other.Other.GameObject}");
    }

    /// <summary>
    /// Called when a collision stops with another collider.
    /// </summary>
    /// <param name="other">The collision stop data.</param>
    public void OnCollisionStop(CollisionStop other)
    {
        Log.Info($"{EntityName} stopped colliding with {other.Other.GameObject}");
    }

    /// <summary>
    /// Called when the component is disabled.
    /// </summary>
    protected override void OnDisabled()
    {
        base.OnDisabled();
        // Handle when the component is disabled
    }

    /// <summary>
    /// Called when the component is destroyed. Cleans up resources.
    /// </summary>
    protected override void OnDestroy()
    {
        base.OnDestroy();
        // Cleanup when the component is destroyed
    }

    /// <summary>
    /// Handles player input. This is a placeholder for actual input handling logic.
    /// </summary>
    private void HandlePlayerInput()
    {
        // Implement input handling logic here
    }

    /// <summary>
    /// Handles physics-related movement. This is a placeholder for actual physics logic.
    /// </summary>
    private void ApplyPhysicsMovement()
    {
        // Implement physics movement logic here
    }

    /// <summary>
    /// Called when the player uses the default interaction key (default is "E").
    /// </summary>
    /// <param name="tr">The scene trace result.</param>
    /// <param name="player">The player interacting with the entity.</param>
    public void InteractUse(SceneTraceResult tr, GameObject player)
    {
        Log.Info($"{player} used {EntityName} with the default interaction.");
    }

    /// <summary>
    /// Called when the player uses the special interaction key (default is "R").
    /// </summary>
    /// <param name="tr">The scene trace result.</param>
    /// <param name="player">The player interacting with the entity.</param>
    public void InteractSpecial(SceneTraceResult tr, GameObject player)
    {
        Log.Info($"{player} used {EntityName} with a special interaction.");
    }

    /// <summary>
    /// Called when the player uses the Attack 1 interaction key (default is "Mouse 1").
    /// </summary>
    /// <param name="tr">The scene trace result.</param>
    /// <param name="player">The player interacting with the entity.</param>
    public void InteractAttack1(SceneTraceResult tr, GameObject player)
    {
        Log.Info($"{player} used {EntityName} with an Attack 1 interaction.");
    }

    /// <summary>
    /// Called when the player uses the Attack 2 interaction key (default is "Mouse 2").
    /// </summary>
    /// <param name="tr">The scene trace result.</param>
    /// <param name="player">The player interacting with the entity.</param>
    public void InteractAttack2(SceneTraceResult tr, GameObject player)
    {
        Log.Info($"{player} used {EntityName} with an Attack 2 interaction.");
    }
}