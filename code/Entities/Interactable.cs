using Sandbox;


namespace Entity.Interactable
{

  /// <summary>
  /// Interface for interactable objects.
  /// Inherit from this interface to create interactable objects.
  /// </summary>
  public interface IInteractable
  {
    /// <summary>
    /// Called when the player uses the default interaction key. Default key is "E". Action is "Use".
    /// </summary>
    void InteractUse( SceneTraceResult tr, GameObject player );
    /// <summary>
    /// Called when the player uses the special interaction key. Default key is "R". Action is "Use Sepcial".
    /// </summary>
    void InteractSpecial( SceneTraceResult tr, GameObject player ){ }
    /// <summary>
    /// Called when the player uses the Attack 1 interaction key. Default key is "Mouse 1". Action is "attack1".
    /// </summary>
    void InteractAttack1( SceneTraceResult tr, GameObject player ){ }
    /// <summary>
    /// Called when the player uses the Attack 2 interaction key. Default key is "Mouse 2". Action is "attack2".
    /// </summary>
    void InteractAttack2( SceneTraceResult tr, GameObject player ){ }
  }

  public class Interactable : Component, IInteractable
  {
    public virtual void InteractUse( SceneTraceResult hit, GameObject player )
    {
      // Default interaction behavior
      Log.Info( "Interacted with " + player.Name );
    }
  }
}