using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

// Classes should inherit from this interface if they are undoable with the "Z" key by default
public interface IUndoable
{
	void Undo();
}

public sealed class PropToolManager : Component
{
	[Property] GameObject PropPrefab { get; set; }
	[Property] GameObject Screen { get; set; }
	[Property] public int PropLimit { get; set; } = 10;


	// List to store currently spawned props.
	List<GameObject> Props { get; set; } = new List<GameObject>();

	/// <summary>
	/// Represents an action related to prop management that can be undone.
	/// </summary>
	public class PropAction : IUndoable
	{
		private readonly PropToolManager _propToolManager;
		private GameObject Prop { get; }
		private string Name { get; }
		private Vector3 Position { get; }
		private Rotation Rotation { get; }

		public PropAction( PropToolManager propToolManager, GameObject prop, string name )
		{
			_propToolManager = propToolManager;
			Prop = prop;
			Position = prop.Transform.Position;
			Rotation = prop.Transform.Rotation;
			Name = name;
		}

		/// <summary>
		/// Undoes the prop creation by destroying the prop and removing it from the list.
		/// </summary>
		public void Undo()
		{
			Prop.Destroy();
			_propToolManager.Props.Remove( Prop );
			_propToolManager.Screen?.Components.Get<PlayerHUD>()?.Notify( PlayerHUD.NotificationType.Info, $"Undo prop {Name}" );
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
		foreach ( var prop in Props )
		{
			prop.Destroy();
		}
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
		Vector3 SpawnOffset = GameObject.Components.Get<PlayerInteraction>().CameraComponent.Transform.World.Forward * -50f;

		// Attempt to get the player's forward line trace position
		Vector3? nullablePlayerPos = GameObject.Components.Get<PlayerInteraction>()?.ForwardLineTrace();
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
	/// Stores a history of actions that can be undone.
	/// </summary>
	List<IUndoable> History { get; set; } = new List<IUndoable>();

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
