using GameSystems.Player;
using Sandbox.Citizen;

namespace Entity.Interactable.Props
{
  public sealed class Sitable : Component, IInteractable
  {

    [HostSync] bool IsOccupied { get; set; } = false;
    [HostSync] GameObject Occupant { get; set; }
    [Property] GameObject SeatSpot { get; set; }

    public void InteractUse( SceneTraceResult tr, GameObject player )
    {
      // If the seat is occupied, return
      if ( IsOccupied && Occupant.Id != player.Id ) return;
      if ( IsOccupied && Occupant.Id == player.Id ) { Stand( player ); return; }
      Sit( player );
    }

    [Broadcast]
    public void Sit( GameObject player )
    {
      // Get the necessary components
      if ( player.Components.Get<MovementController>() is not MovementController movementController ||
          player.Components.Get<CharacterController>() is not CharacterController characterController ||
          player.Components.Get<CitizenAnimationHelper>() is not CitizenAnimationHelper animationHelper )
      {
        return;
      }
      // Check if they are already sitting or they cannot move
      if ( movementController.Seat == this || movementController.DisabledMovement ) return;

      // Sit the player
      // Handle the player's movement
      characterController.Velocity = Vector3.Zero;
      characterController.IsOnGround = true;
      movementController.Seat = this;
      movementController.DisabledMovement = true;
      player.Transform.Position = SeatSpot.Transform.Position;

      animationHelper.Sitting = CitizenAnimationHelper.SittingStyle.Chair;
      animationHelper.IsSitting = true;


      // TODO need animations

      // Parent the player to the seat
      player.SetParent( SeatSpot );

      // Handle the seat
      IsOccupied = true;
      Occupant = player;
    }

    [Broadcast]
    public void Stand( GameObject player )
    {
      // Check if the player is the occupant
      if ( player.Id != Occupant.Id ) return;

      // Get the player's movement controller
      if ( player.Components.Get<MovementController>() is not MovementController movementController ||
          player.Components.Get<CitizenAnimationHelper>() is not CitizenAnimationHelper animationHelper )
      {
        return;
      }

      // Stand the player
      // Handle the player's movement
      movementController.Seat = null;
      movementController.DisabledMovement = false;
      // TODO perhaps a differnet way to reposition the player?
      player.Transform.Position = SeatSpot.Transform.Position + new Vector3( 0, 0, 64 );

      // Unparent the player from the seat
      player.SetParent( null );

      animationHelper.Sitting = CitizenAnimationHelper.SittingStyle.None;
      animationHelper.IsSitting = false;

      // Handle the seat
      IsOccupied = false;
      Occupant = null;
    }
  }
}