using Sandbox;

/// <summary>
/// A sealed class that manages the prop logic and interaction .
/// </summary>
public sealed class PropLogic : Component, IInteractable
{

	/// <summary>
	/// Updates the prop model by assigning a new model to the prop component.
	/// </summary>
	/// <param name="modelname">The name of the model to load and assign to the prop.</param>
	public void UpdatePropModel( string modelname )
	{
		var prop = this.Components.GetOrCreate<Prop>();
		prop.Model = Model.Load( modelname );
	}

	/// <summary>
	/// Updates the prop collider by assigning the model bounds to a model collider component.
	/// </summary>
	/// <param name="modelname">The name of the model to load and assign to the collider.</param>
	public void UpdatePropCollider( string modelname )
	{
		var ModelCollider = this.Components.GetOrCreate<ModelCollider>();
		ModelCollider.Model = Model.Load( modelname );
	}

}
