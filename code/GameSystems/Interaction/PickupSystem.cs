using Sandbox;

namespace GameSystems.Interaction
{
    public class PickupSystem : Component
    {

		/// <summary>
		/// Properties for physics holding props
		/// </summary>
		[Property, Sync] 
        public GameObject HoldingArea { get; set; }
        [Property]
        public readonly float LerpTime = 0.01f;

        private float interactRange;
		private GameObject heldObject;
		private Rigidbody heldObjectRigidbody;
		private PlayerController playerController;

        public PickupSystem(float interactRange)
        {
            this.interactRange = interactRange;
        }
		protected override void OnAwake()
		{
			// parent the holding area to the players camera
			playerController = GameObject.Components.Get<PlayerController>();
		}
		public bool IsHoldingObject(){
			return heldObject != null;
		}
        [Broadcast]
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
				heldObjectRigidbody.PhysicsBody.LinearDrag = 100f;

				Log.Info("Picking up object");
				//heldObjectRigidbody.PhysicsBody.Enabled = false;
				heldObjectRigidbody.GameObject.SetParent(HoldingArea);
				heldObject = pickedUpObject;
			}
		}
		public void DropPickup()
		{
			Log.Info("Dropping object");
			heldObjectRigidbody.Gravity = true;
			//m_heldObjectRigidbody.ClearForces();
			heldObjectRigidbody.PhysicsBody.LinearDrag = 1f;

			heldObjectRigidbody.GameObject.SetParent(null);
			heldObject = null;
		}
		public void MoveHeldObject()
		{
			if (heldObject != null)
			{
				SetHoldingArea();
				float dist = Vector3.DistanceBetween(heldObject.Transform.Position, HoldingArea.Transform.Position);
				if (dist > 1f)
				{
					Log.Info("Moving object");
					heldObjectRigidbody.Transform.LerpTo(HoldingArea.Transform.World, 0.05f);
				}
				// Could be extended with rotating an item
				if (dist > interactRange)
				{
					Log.Info("Throwing object");
					DropPickup();
				}
			}
		}
		private void SetHoldingArea()
		{
			var eyePos = playerController.Transform.Position + new Vector3(0, 0, playerController.EyeHeight);
			var eyeAngles = playerController.EyeAngles.Forward;
			// rotate the target target vector according to the camera rotation from the eye position
			var targetVec = eyePos + eyeAngles * (interactRange / 2);

			HoldingArea.Transform.Position = HoldingArea.Transform.Position.LerpTo(targetVec, 0.05f);
		}
    }
}