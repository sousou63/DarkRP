using Sandbox;

/// <summary>
/// Interface for interactable objects.
/// Inherit from this interface to create interactable objects.
/// </summary>
public interface IInteractable
{
  /// <summary>
  /// Called when the player uses the default interaction key. Default key is "E". Action is "Use".
  /// </summary>
  void Interact( SceneTraceResult tr, GameObject player )
  {

  }

  /// <summary>
  /// Called when the player uses the special interaction key. Default key is "F2". Action is "Use Sepcial".
  /// </summary>
  void InteractSpecial( SceneTraceResult tr, GameObject player )
  {

  }
}

public class Interactable : Component, IInteractable
{
  public virtual void Interact( SceneTraceResult hit, GameObject player )
  {
    // Default interaction behavior
    Log.Info( "Interacted with " + player.Name );
  }
}
