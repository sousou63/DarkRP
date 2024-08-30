using Sandbox;
using Sandbox.Entities.Interactable.Props;
using Sandbox.Entity;
using Sandbox.UI;

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
		private readonly PropToolManager _propToolManager;
		private GameObject _prop { get; }
		private string _name { get; }
		private Vector3 _position { get; }
		private Rotation _rotation { get; }

		public PropAction( PropToolManager propToolManager, GameObject prop, string name )
		{
			_propToolManager = propToolManager;
			_prop = prop;
			_position = prop.Transform.Position;
			_rotation = prop.Transform.Rotation;
			_name = name;
		}

		/// <summary>
		/// Undoes the prop creation by destroying the prop and removing it from the list.
		/// </summary>
		public void Undo()
		{
			_prop.Destroy();
			_propToolManager.Props.Remove( _prop );
			_propToolManager.Screen?.Components.Get<PlayerHUD>()?.Notify( PlayerHUD.NotificationType.Info, $"Undo prop {_name}" );
		}
	}
}
