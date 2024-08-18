using Sandbox;
using GameSystems.Player;

namespace GameSystems.Interaction
{
    public class PickupSystem : Component
    {

		/// <summary>
		/// Properties for physics holding props
		/// </summary>

		private GameObject holdingArea;

        private float interactRange;
		private float rotateSpeed = 0.01f;
		private GameObject heldObject;
		private Rigidbody heldObjectRigidbody;
		private Vector3 heldObjectCenter;
		private Player.MovementController playerController;

        public PickupSystem(float interactRange, Player.MovementController playerController, GameObject holdingArea)
        {
            this.interactRange = interactRange;
			this.playerController = playerController;
			this.holdingArea = holdingArea;
		}
		public bool IsHoldingObject(){
			return (heldObject != null && heldObjectRigidbody != null);
		}

		public void HandlePickup(GameObject pickedUpObject)
		{
			Rigidbody rb = pickedUpObject.Components.Get<Rigidbody>();
			if (rb != null)
			{
				// remove the earlier parent of the pickedUpObject 
				// i.e if someone else is holding it, remove it from their hands
				if (pickedUpObject.Parent != null)
				{
					pickedUpObject.SetParent(null);
					rb.GameObject.SetParent(null);
				}
				// maybe some other logic is preferred. Maybe not being able to steal items from others

				heldObjectRigidbody = rb;
				heldObjectRigidbody.Gravity = false;
				heldObjectRigidbody.ClearForces();

				// Get the bounds of the object
				var bounds = pickedUpObject.GetBounds();
				heldObjectCenter = bounds.Center;

				heldObjectRigidbody.GameObject.SetParent(holdingArea);
				heldObject = pickedUpObject;
			}
		}
		public void DropPickup()
		{
			Log.Info("Dropping object");
			heldObjectRigidbody.Gravity = true;
			//heldObjectRigidbody.PhysicsBody.LinearDrag = 1f;

			UnlockHeldObject();
			heldObjectRigidbody.GameObject.SetParent(null);
			heldObject = null;
		}
		public void MoveHeldObject()
		{
			if (heldObject != null)
			{
				SetHoldingArea();
				float dist = Vector3.DistanceBetween(heldObject.Transform.Position, holdingArea.Transform.Position);
				if (dist > 1.5f*interactRange) { DropPickup(); return; }

				if (dist > 1f)
				{
					heldObjectRigidbody.Transform.Position = holdingArea.Transform.Position;
				}
				// Could be extended with rotating an item
			}
		}
		public void RotateHeldObject()
		{
			playerController.EyesLocked = true;
			heldObject.Transform.Local = heldObject.Transform.Local.RotateAround(heldObjectCenter, Input.AnalogLook);
		}
		public void UnlockHeldObject()
		{
			playerController.EyesLocked = false;
		}
		private void SetHoldingArea()
		{
			var eyePos = playerController.Transform.Position + new Vector3(0, 0, playerController.EyeHeight);
			var eyeAngles = playerController.EyeAngles.Forward;
			// rotate the target target vector according to the camera rotation from the eye position
			var targetVec = eyePos + eyeAngles * (interactRange / 2);

			holdingArea.Transform.Position = holdingArea.Transform.Position.LerpTo(targetVec, 0.15f);
		}
    }
}
