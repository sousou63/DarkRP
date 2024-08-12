using Sandbox;

/// <summary>
/// Interface for interactable objects.
/// Inherit from this interface to create interactable objects.
/// </summary>
public interface IInteractable
{
  void Interact( SceneTraceResult tr, GameObject player );
}

public class Interactable : Component, IInteractable
{
  public virtual void Interact( SceneTraceResult hit, GameObject player )
  {
    // Default interaction behavior
    Log.Info( "Interacted with " + player.Name );
  }
}
