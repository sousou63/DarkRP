using System;
using System.Collections.Generic;
using Entity.Interactable.Props;
using Sandbox.UI;
using Utils;

namespace Sandbox.Entities.Interactable.Props
{
	public sealed class PropToolManager : Component
	{
		[Property] public GameObject PropPrefab { get; set; }
		[Property] public GameObject Screen { get; set; }
		[Property] public int PropLimit { get; set; } = 10;
		[Property] public float SpawnProtectionTimeWindow { get; set; } = 1;
		[Property] public bool UseCloudProps { get; set; } = true;

		private CameraComponent _camera;

		// List to store currently spawned props.
		public List<GameObject> Props { get; set; } = new List<GameObject>();

		// History for undo actions
		private List<IUndoable> History { get; set; } = new List<IUndoable>();

		// for spawn protection
		private TimeSince timeSinceLastClick;

		protected override void OnAwake()
		{
			_camera = Scene.GetAllComponents<CameraComponent>().FirstOrDefault( x => x.IsMainCamera );
		}

		protected override void OnFixedUpdate()
		{
			base.OnFixedUpdate();

			// Handle undo input
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

		public void SpawnProp( string modelname )
		{
			// Check if the time since the last spawn is less than the allowed time window
			if ( timeSinceLastClick <= SpawnProtectionTimeWindow )
			{

				Log.Info( timeSinceLastClick );
				// Trigger protection
				TriggerSpawnProtection();
				return;
			}

			// Check if the prop limit has been reached
			if ( Props.Count >= PropLimit )
			{
				Screen?.Components.Get<PlayerHUD>()?.Notify( PlayerHUD.NotificationType.Warning, $"Vous avez atteint la limite de props ({PropLimit})" );
				return;
			}

			// Calculate spawn offset based on the player's camera position and orientation
			var SpawnOffset = _camera.Transform.World.Forward * -50f;

			Vector3? nullablePlayerPos = TraceUtils.ForwardLineTrace( Scene, _camera.Transform, 100 );
			var playerPos = nullablePlayerPos ?? Vector3.Zero;

			if ( playerPos == Vector3.Zero )
			{
				playerPos = GameObject.Transform.World.Position + GameObject.Transform.Local.Forward * 150;
			}

			playerPos += SpawnOffset;

			// Clone the prop prefab at the calculated position
			var Prop = PropPrefab.Clone( playerPos );
			Prop.Components.Get<PropLogic>().UpdatePropModel( modelname );
			Prop.Components.Get<PropLogic>().UpdatePropCollider( modelname );

			// Spawn the prop on all clients
			Prop.NetworkSpawn();
			Props.Add( Prop );
			History.Add( new PropAction( this, Prop, modelname ) );

			// Notify the player
			Screen?.Components.Get<PlayerHUD>()?.Notify( PlayerHUD.NotificationType.Info, $"Prop {modelname} spawné ({Props.Count}/{PropLimit})" );

			// Reset the timer 
			timeSinceLastClick = 0;
		}

		private void TriggerSpawnProtection()
		{

			// Reset the timer 
			timeSinceLastClick = 0;

			// Log the protection trigger or notify the player
			Screen?.Components.Get<PlayerHUD>()?.Notify( PlayerHUD.NotificationType.Warning, "Spawn protection activated, please slow down !" );
		}

		public void UndoLastAction()
		{
			if ( History.Count > 0 )
			{
				History.Last().Undo();
				History.RemoveAt( History.Count - 1 );
			}
		}

		public int PropCount()
		{
			return Props.Count;
		}

		public void RemoveAllProps()
		{
			Props.ForEach( prop => prop.Destroy() );

			Screen?.Components.Get<PlayerHUD>()?.Notify( PlayerHUD.NotificationType.Info, "Tous vos props ont été supprimés" );
			Props.Clear();
		}

		public GameObject SpawnCloudModel( string cloudModel, Vector3 position, Rotation rotation )
		{
			if ( Props.Count >= PropLimit )
			{
				Screen?.Components.Get<PlayerHUD>()?.Notify( PlayerHUD.NotificationType.Warning, $"Vous avez atteint la limite de props ({PropLimit})" );
				return null;
			}

			Vector3 spawnOffset = GameObject.Transform.Local.Forward * 100f;
			position += spawnOffset;

			GameObject Prop = PropPrefab.Clone( position );
			Prop.Transform.Rotation = rotation;

			var PropHelper = Prop.Components.GetOrCreate<PropHelper>();
			if ( PropHelper != null )
			{
				PropHelper.SetCloudModel( cloudModel );
			}
			else
			{
				Log.Warning( $"Composant PropHelper non trouvé sur le prefab du prop." );
				return null;
			}

			if ( Prop.NetworkSpawn() )
			{
				Props.Add( Prop );
				History.Add( new PropAction( this, Prop, cloudModel ) );
				Screen?.Components.Get<PlayerHUD>()?.Notify( PlayerHUD.NotificationType.Info, $"Modèle de nuage {cloudModel} spawné ({Props.Count}/{PropLimit})" );
				return Prop;
			}
			else
			{
				Log.Warning( "Échec du spawn en réseau du modèle de nuage." );
				return null;
			}
		}

		public string GetPropThumbnail( string propName )
		{
			var thumbnailPath = $"{propName.ToLower().Replace( ".vmdl", "" )}.vmdl_c.png";
			return thumbnailPath;
		}
	}
}
