using Sandbox;
using System;
using GameSystems.Interaction;
using Sandbox.UI;

namespace Entity.Interactable.Props
{
	// Classes should inherit from this interface if they are undoable with the "Z" key by default
	public sealed class PropToolManager : Component
	{
		[Property] public GameObject PropPrefab { get; set; }
		[Property] public GameObject Screen { get; set; }
		[Property] public int PropLimit { get; set; } = 10;


		// List to store currently spawned props.
		public List<GameObject> Props { get; set; } = new List<GameObject>();

		/// <summary>
		/// Stores a history of actions that can be undone.
		/// </summary>
		private List<IUndoable> History { get; set; } = new List<IUndoable>();


		/// <summary>
		/// Called every frame, listens for undo input and attempts to undo the last action if triggered.
		/// </summary>
		protected override void OnUpdate()
		{
			if ( Input.Pressed( "Undo" ) )
			{
				try
				{
					UndoLastAction();
				}
				catch ( Exception e )
				{
					Log.Error( e );
				}
			}
		}

		/// <summary>
		/// Returns the number of props currently spawned and owned by the player.
		/// </summary>
		/// <returns>Number of spawned props.</returns>
		public int PropCount()
		{
			return Props.Count;
		}

		/// <summary>
		/// Removes all props that the player has spawned.
		/// </summary>
		public void RemoveAllProps()
		{
			Props.ForEach( prop => prop.Destroy() );

			Screen?.Components.Get<PlayerHUD>()?.Notify( PlayerHUD.NotificationType.Info, $"Removed all your props" );
			Props.Clear();
		}

		/// <summary>
		/// Attempts to spawn a prop at the player's forward line trace position, 
		/// otherwise spawns it 50 units in front of the player.
		/// </summary>
		/// <param name="modelname">The name of the model to spawn.</param>
		public void SpawnProp( string modelname )
		{
			// Check if the prop limit has been reached
			if ( Props.Count >= PropLimit )
			{
				Screen?.Components.Get<PlayerHUD>()?.Notify( PlayerHUD.NotificationType.Warning, $"You've reached the Prop Limit ({PropLimit})" );
				return;
			}

			// Calculate spawn offset based on the player's camera position and orientation
			Vector3 SpawnOffset = GameObject.Components.Get<InteractionSystem>().CameraComponent.Transform.World.Forward * -50f;

			// Attempt to get the player's forward line trace position
			Vector3? nullablePlayerPos = GameObject.Components.Get<InteractionSystem>()?.ForwardLineTrace();
			Vector3 playerPos = nullablePlayerPos ?? Vector3.Zero;

			// If the line trace failed, use a fallback position in front of the player
			if ( playerPos == Vector3.Zero )
			{
				playerPos = GameObject.Transform.World.Position + GameObject.Transform.Local.Forward * 150;
			}

			// TODO : MAKE A DYNAMIC OFFSET
			// Apply the spawn offset to the final position 
			playerPos = playerPos + SpawnOffset;

			// Clone the prop prefab at the calculated position
			GameObject Prop = PropPrefab.Clone( playerPos );

			// Update the prop's model and collider to match the specified model name
			Prop.Components.Get<PropLogic>().UpdatePropModel( modelname );
			Prop.Components.Get<PropLogic>().UpdatePropCollider( modelname );

			// TODO: Consider wrapping the entire process in a try-catch to handle exceptions and clean up resources

			// Spawn the prop on all clients
			Prop.NetworkSpawn();
			Props.Add( Prop );
			History.Add( new PropAction( this, Prop, modelname ) );

			// Notify the player that the prop has been spawned
			Screen?.Components.Get<PlayerHUD>()?.Notify( PlayerHUD.NotificationType.Info, $"Spawned prop {modelname} ({Props.Count}/{PropLimit})" );
		}
		
		/// <summary>
		/// Spawns a cloud-based model at a specified position and rotation.
		/// </summary>
		/// <param name="cloudModel">The identifier of the cloud model to spawn.</param>
		/// <param name="position">The position in the world where the model should be spawned.</param>
		/// <param name="rotation">The rotation to apply to the model after spawning.</param>
		public GameObject SpawnCloudModel(string cloudModel, Vector3 position, Rotation rotation)
		{
			if (Props.Count >= PropLimit)
			{
				Screen?.Components.Get<PlayerHUD>()?.Notify(PlayerHUD.NotificationType.Warning, $"You've reached the Prop Limit ({PropLimit})");
				return null;
			}

			// Calculate a spawn position in front of the player
			Vector3 spawnOffset = GameObject.Transform.Local.Forward * 100f;
			position += spawnOffset;

			GameObject Prop = PropPrefab.Clone(position);
			Prop.Transform.Rotation = rotation;

			var PropHelper = Prop.Components.GetOrCreate<PropHelper>();
			if (PropHelper != null)
			{
				PropHelper.SetCloudModel(cloudModel);
			}
			else
			{
				Log.Warning($"PropHelper component not found on the prop prefab.");
				return null;
			}

			if (Prop.NetworkSpawn())
			{
				Props.Add(Prop);
				History.Add(new PropAction(this, Prop, cloudModel));
				Screen?.Components.Get<PlayerHUD>()?.Notify(PlayerHUD.NotificationType.Info, $"Spawned cloud model {cloudModel} ({Props.Count}/{PropLimit})");
				return Prop;
			}
			else
			{
				Log.Warning("Failed to network spawn the cloud model.");
				return null;
			}
		}

		/// <summary>
		/// Undoes the last action performed by the player.
		/// </summary>
		public void UndoLastAction()
		{
			if ( History.Count > 0 )
			{
				History.Last().Undo();
				History.RemoveAt( History.Count - 1 );
			}
		}
	}
}
