using GameSystems.Player;
using Entity;

/// <summary>
/// Represents a money entity that players can interact with to collect money.
/// Inherits from BaseEntity for shared functionality.
/// </summary>
[Library("money", Title = "Money")]
public sealed class Money : BaseEntity
{
    /// <summary>
    /// Amount of money this entity represents.
    /// </summary>
    [Property, Sync] public int Amount { get; set; } = 100;

    /// <summary>
    /// Called when the component is first created and added to a GameObject.
    /// Initializes the component and sets up the model and entity name.
    /// </summary>
    protected override void OnStart()
    {
        base.OnStart();
        EntityName = "Money";
    }

    /// <summary>
    /// Handles interaction when the player uses the default interaction key (e.g., "E").
    /// This method adds money to the player's stats and destroys the money entity.
    /// </summary>
    public override void InteractUse(SceneTraceResult tr, GameObject player)
    {
        Log.Info("Interacting with money");

        var playerStats = player.Components.Get<Stats>();
        if (playerStats != null)
        {
            playerStats.AddMoney(Amount);
            Sound.Play("audio/money.sound");
            DestroyMoney();
        }
    }

    /// <summary>
    /// Destroys the money entity after it has been collected.
    /// </summary>
    [Broadcast]
    public void DestroyMoney()
    {
        this.GameObject.Destroy();
    }
}
