using Sandbox.Entity;
using Sandbox.GameSystems.Player;


public sealed class AtmLogic : BaseEntity, Component.INetworkListener
{
	[Property, Group( "References" )] public AtmMenu AtmMenu { get; set; }
	public override void InteractUse( SceneTraceResult tr, GameObject player )
	{
		AtmMenu.Enabled = true;

	}
}

