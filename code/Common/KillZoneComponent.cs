using Grubs.Pawn;

namespace Grubs.Common;

[Title( "Grubs - Kill Zone" ), Category( "Grubs" )]
public class KillZoneComponent : Component, Component.ITriggerListener
{
	public void OnTriggerEnter( Collider other )
	{
		if ( other.GameObject.Components.TryGet( out Grub grub, FindMode.EverythingInSelfAndAncestors ) )
			grub.Health.TakeDamage( GrubsDamageInfo.FromKillZone( 9999 ), true );

		if ( other.GameObject.Tags.Has( "projectile" ) && other.Transform.Position != 0f )
			other.GameObject.Destroy();
	}

	public void OnTriggerExit( Collider other )
	{
	}
}
