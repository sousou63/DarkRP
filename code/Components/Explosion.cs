using System;
using Sandbox;

namespace Scenebox;

public sealed class Explosion : Component, Component.ITriggerListener
{
	[RequireComponent] SphereCollider Collider { get; set; }

	public float Force { get; set; } = 100_000f;

	public void OnTriggerEnter( Collider other )
	{
		var obj = other.GameObject;
		var position = other.Transform.Position;

		if ( obj.Tags.HasAny( "grabbed", "map" ) ) return;

		var distance = (position - Transform.Position).Length;
		var forceDirection = (position - Transform.Position).Normal;

		float damage = 100f * Math.Clamp( 1f - distance / Collider.Radius, 0f, 1f );
		Vector3 force = forceDirection * Force * (1f - distance / Collider.Radius);

		if ( obj.Root.Components.TryGet<Player>( out var player ) )
		{
			player.Damage( damage );
		}
		else if ( obj.Components.TryGet<PropHelper>( out var propHelper ) )
		{
			propHelper.BroadcastAddDamagingForce( force, damage );
		}
		else if ( obj.Components.TryGet<Rigidbody>( out var rigidbody ) )
		{
			obj.Network.TakeOwnership();
			rigidbody.ApplyForce( force );
		}
	}
}
