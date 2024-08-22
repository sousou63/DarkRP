using Sandbox;
using System;

namespace Entity.Interactable.Props
{
    /// <summary>
    /// A component that manages the interaction and networking of prop entities, including cloud models.
    /// </summary>
    public sealed class PropHelper : Component, Component.ICollisionListener
    {
        /// <summary>
        /// The prop associated with this helper.
        /// </summary>
        Prop Prop;

        /// <summary>
        /// The unique identifier of the creator of this prop.
        /// </summary>
        [Sync] public Guid CreatorId { get; set; } = Guid.Empty;

        /// <summary>
        /// The health of the prop.
        /// </summary>
        [Sync] public float Health { get; set; } = 1;

        /// <summary>
        /// A networked dictionary of bodies associated with the prop.
        /// </summary>
        [Sync] NetDictionary<int, BodyInfo> NetworkedBodies { get; set; } = new();

        /// <summary>
        /// The identifier of the cloud model associated with this prop. When changed, the model is initialized.
        /// </summary
        [Sync, Change("InitCloudModel")] string CloudModel { get; set; } = "";

        /// <summary>
        /// The physics component associated with the prop.
        /// </summary>
        ModelPhysics Physics;

        /// <summary>
        /// The rigidbody component associated with the prop.
        /// </summary>
        Rigidbody Rigidbody;

        /// <summary>
        /// Struct representing body information, including its type and transform.
        /// </summary>
        struct BodyInfo
        {
            public PhysicsBodyType Type;
            public Transform Transform;
        }

        /// <summary>
        /// Called when the component starts. Initializes the prop and its related components.
        /// </summary>
        protected override void OnStart()
        {
            Prop = Components.Get<Prop>();
            Health = Prop?.Health ?? 0;
            Physics = Components.Get<ModelPhysics>(FindMode.EverythingInSelf);
            Rigidbody = Components.Get<Rigidbody>();

            InitCloudModel();
        }

        /// <summary>
        /// Initializes the cloud model for the prop by fetching and loading it from the cloud.
        /// </summary>
        async void InitCloudModel()
        {
            if (string.IsNullOrWhiteSpace(CloudModel)) return;

            var package = await Package.Fetch(CloudModel, false);
            await package.MountAsync();

            var model = Model.Load(package.GetMeta("PrimaryAsset", ""));
            if (model is null) return;

            if (Prop.IsValid())
            {
                Prop.Enabled = false;
                Prop.Model = model;
                Prop.Enabled = true;
            }
        }

        /// <summary>
        /// Sets the cloud model for this prop and initializes it.
        /// </summary>
        /// <param name="cloudModel">The identifier of the cloud model to set.</param>
        [Broadcast]
        public void SetCloudModel(string cloudModel)
        {
            CloudModel = cloudModel;
            InitCloudModel();
        }

        /// <summary>
        /// Handles the start of a collision with another entity.
        /// </summary>
        /// <param name="other">The collision information with the other entity.</param>
        public void OnCollisionStart(Collision other)
        {
            // Handle collision logic here
        }
    }
}