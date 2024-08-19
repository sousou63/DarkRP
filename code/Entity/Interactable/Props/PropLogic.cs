using Sandbox;

namespace Entity.Interactable.Props 
{
	/// <summary>
	/// A sealed class that manages the prop logic and interaction .
	/// </summary>
	public sealed class PropLogic : BaseEntity
	{

		public override void InteractUse( SceneTraceResult tr, GameObject player )
		{
			// Should probably be picked up in this case 
			Log.Info( "Interacting with prop" );
		}
		/// <summary>
		/// Updates the prop model by assigning a new model to the prop component.
		/// </summary>
		/// <param name="ModelName">The name of the model to load and assign to the prop.</param>
		public void UpdatePropModel( string ModelName )
		{
			var prop = this.Components.GetOrCreate<Prop>();
			prop.Model = Model.Load( ModelName );
		}

		/// <summary>
		/// Updates the prop collider by assigning the model bounds to a model collider component.
		/// </summary>
		/// <param name="ModelName">The name of the model to load and assign to the collider.</param>
		public void UpdatePropCollider( string ModelName )
		{
			var ModelCollider = this.Components.GetOrCreate<ModelCollider>();
			ModelCollider.Model = Model.Load( ModelName );
		}

	}

	/// <summary>
	/// Represents an action related to prop management that can be undone.
	/// </summary>
	public class PropAction : IUndoable
	{
		private readonly PropToolManager propToolManager;
		private GameObject prop { get; }
		private string name { get; }
		private Vector3 position { get; }
		private Rotation rotation { get; }

		public PropAction( PropToolManager PropToolManager, GameObject Prop, string Name )
		{
			propToolManager = PropToolManager;
			prop = Prop;
			position = Prop.Transform.Position;
			rotation = Prop.Transform.Rotation;
			name = Name;
		}

		/// <summary>
		/// Undoes the prop creation by destroying the prop and removing it from the list.
		/// </summary>
		public void Undo()
		{
			prop.Destroy();
			propToolManager.Props.Remove( prop );
			propToolManager.Screen?.Components.Get<PlayerHUD>()?.Notify( PlayerHUD.NotificationType.Info, $"Undo prop {name}" );
		}
	}
}
