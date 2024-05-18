using Grubs.Pawn;

namespace Grubs.Common;

[Title( "Grubs - Kill Zone" ), Category( "Grubs" )]
public class KillZoneComponent : Component, Component.ITriggerListener
{
	public void OnTriggerEnter( Collider other )
	{
		if ( other.GameObject.Components.TryGet( out Grub grub, FindMode.EverythingInSelfAndAncestors ) )
			grub.Health.TakeDamage( GrubsDamageInfo.FromKillZone( 9999 ), true );

		DestroyObjectWithTags( other.GameObject, "projectile", "drop" );
	}

	private void DestroyObjectWithTags( GameObject go, params string[] args )
	{
		foreach ( var tag in args )
		{
			if ( go.Tags.Has( tag ) && go.Transform.Position != 0f )
				go.Destroy();
		}
	}

	public void OnTriggerExit( Collider other )
	{
	}
}
