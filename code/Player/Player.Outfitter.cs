//using Softsplit;

namespace Sandbox.GameSystems.Player;

/// <summary>
/// A component that handles what a player wears.
/// </summary>
public partial class Player
{
	/// <summary>
	/// We store the player's avatar over the network so everyone knows what everyone looks like.
	/// </summary>
	[Sync] public string Avatar { get; set; }

	/// <summary>
	/// Grab the player's avatar data.
	/// </summary>
	/// <param name="owner"></param>
	private void OnNetworkSpawnOutfitter(Connection owner)
	{
		if ( !Components.TryGet<SkinnedModelRenderer>( out var model ) )
		{
			return;
		}

		Avatar = owner.GetUserData("avatar");

		var container = new ClothingContainer();
		container.Deserialize(Avatar);
		container.Apply(model);
	}
}
