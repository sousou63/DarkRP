using Sandbox;
using GameSystems.Player;
using Entity.Interactable;
using GameSystems.Interaction;


namespace Entity
{

	/// <summary>
	/// Represents a generic base component that provides common functionality 
	/// for various types of interactable entities such as dropped money, printers, food, etc.
	/// </summary>
	[Library( "base_entity", Title = "Base Entity" )]
	public class BaseEntity : Component, IInteractable
	{
		/// <summary>
		/// Gets or sets the health of the entity.
		/// </summary>
		[Property] public float Health { get; set; } = 100f;

		/// <summary>
		/// Gets or sets the name of the entity.
		/// </summary>
		[Property] public string EntityName { get; set; } = "Base Entity";

		/// <summary>
		/// Gets or sets the owner of the entity.
		/// </summary>
		public PlayerConnObject Owner { get; set; }

		/// <summary>
		/// Gets or sets whether the entity can be picked up by players.
		/// </summary>
		[Property] public bool CanBePickedUp { get; set; } = true;

		/// <summary>
		/// Called when the component is first created and added to a GameObject.
		/// Initializes the component and sets up necessary physics.
		/// </summary>
		protected override void OnStart()
		{
			base.OnStart();
			Log.Info( $"{EntityName} has been initialized." );
			SetupPhysics();

			// Ensure the entity has the interact tag to be recognized by the InteractionSystem
			GameObject.Tags.Add( "Interactable" );
		}

		/// <summary>
		/// Applies damage to the entity and checks if it should be destroyed.
		/// </summary>
		/// <param name="damage">The amount of damage to apply.</param>
		public void TakeDamage( float damage )
		{
			Health -= damage;
			Log.Info( $"{EntityName} took {damage} damage. Health is now {Health}." );

			if ( Health <= 0 )
			{
				OnDestroyed();
			}
		}

		/// <summary>
		/// Called when the entity's health reaches zero. Disables the component.
		/// </summary>
		protected void OnDestroyed()
		{
			Log.Info( $"{EntityName} has been destroyed." );
			Enabled = false; // Disables the component
		}

		/// <summary>
		/// Handles interaction when the player uses the default interaction key (e.g., "E").
		/// </summary>
		public virtual void InteractUse( SceneTraceResult tr, GameObject player )
		{
			Log.Info( $"{player.Name} used {EntityName} with the default interaction." );
		}

		/// <summary>
		/// Handles interaction when the player uses the special interaction key (e.g., "R").
		/// </summary>
		public virtual void InteractSpecial( SceneTraceResult tr, GameObject player )
		{
			Log.Info( $"{player.Name} used {EntityName} with a special interaction." );
		}

		/// <summary>
		/// Handles interaction when the player uses the Attack 1 key (e.g., "Mouse 1").
		/// </summary>
		public virtual void InteractAttack1( SceneTraceResult tr, GameObject player )
		{
			Log.Info( $"{player.Name} used {EntityName} with an Attack 1 interaction." );
		}

		/// <summary>
		/// Handles interaction when the player uses the Attack 2 key (e.g., "Mouse 2").
		/// </summary>
		public virtual void InteractAttack2( SceneTraceResult tr, GameObject player )
		{
			if ( CanBePickedUp )
			{
				player.Components.Get<InteractionSystem>().TryPickup(this.GameObject);
			}
		}

		/// <summary>
		/// Sets up physics properties for the entity, such as colliders and collision groups.
		/// </summary>
		private void SetupPhysics()
		{
			// Setup physics, if necessary.
		}

		/// <summary>
		/// Spawns a new "entity" as a component on a GameObject.
		/// </summary>
		/// <param name="gameObject">The GameObject to add the entity to.</param>
		/// <param name="owner">The player who owns this entity.</param>
		public static void SpawnEntity( GameObject gameObject, PlayerConnObject owner )
		{
			var entity = gameObject.Components.GetOrCreate<BaseEntity>();
			entity.Owner = owner;
			Log.Info( $"Spawned entity {entity.EntityName} on GameObject {gameObject} for player {owner.Name}." );
		}

	}
}
