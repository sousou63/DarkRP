using GameSystems.Player;

namespace Entity.Interactable.Props
{
  public sealed class Sitable : Component, IInteractable {

    [Sync] bool IsOccupied { get; set; } = false;
    [Sync] GameObject Occupant { get; set; }
    [Property] GameObject SeatSpot { get; set; }

    public void InteractUse( SceneTraceResult tr, GameObject player )
    {
      // If the seat is occupied, return
      if ( IsOccupied ) return;
      Sit( player );
    }

    public void Sit( GameObject player)
    {
      // Get the player's movement controller
      var movementController = player.Components.Get<MovementController>();
      if ( movementController is null ) return;
      // Check if they are already sitting or they cannot move
      if ( movementController.Seat == this || movementController.DisabledMovement ) return;

      // Sit the player
      // Handle the player's movement
      movementController.Seat = this;
      movementController.DisabledMovement = true;
      player.Transform.Position = SeatSpot.Transform.Position;
      // TODO need animations

      // Parent the player to the seat
      player.SetParent( SeatSpot );

      // Handle the seat
      IsOccupied = true;
      Occupant = player;
    }

    public void Stand( GameObject player )
    {
      // Check if the player is the occupant
      if ( player.Id != Occupant.Id ) return;

      // Get the player's movement controller
      var movementController = player.Components.Get<MovementController>();
      if ( movementController is null ) return;

      // Stand the player
      // Handle the player's movement
      movementController.Seat = null;
      movementController.DisabledMovement = false;
      // TODO perhaps a differnet way to reposition the player?
      player.Transform.Position = SeatSpot.Transform.Position + new Vector3( 0, 0, 64 );

      // Unparent the player from the seat
      player.SetParent( null );

      // Handle the seat
      IsOccupied = false;
      Occupant = null;
    }
  }
}