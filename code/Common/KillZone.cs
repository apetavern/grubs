using Grubs.Pawn;

namespace Grubs.Common;

[Title( "Grubs - Kill Zone" ), Category( "Grubs" )]
public class KillZone : Component, Component.ITriggerListener
{
	public void OnTriggerEnter( Collider other )
	{
		// kidd: Workaround for ArcProjectile being destroyed immediately for non-owner clients,
		// despite the Transform appearing to be fine. Probably an interp bug, but Transform.ClearInterpolation()
		// in ArcProjectile.OnStart() didn't do SHIT.
		if ( Connection.Local != other.GameObject.Root.Network.OwnerConnection )
			return;

		if ( other.GameObject.Components.TryGet( out Grub grub, FindMode.EverythingInSelfAndAncestors ) )
			grub.Health.TakeDamage( GrubsDamageInfo.FromKillZone( 9999 ), true );

		DestroyObjectWithTags( other.GameObject, "projectile", "drop" );
	}

	private void DestroyObjectWithTags( GameObject go, params string[] args )
	{
		foreach ( var tag in args )
		{
			if ( go.Tags.Has( "ignore_killzone" ) )
				continue;

			if ( go.Tags.Has( tag ) && go.Transform.Position != 0f )
				go.Destroy();
		}
	}

	public void OnTriggerExit( Collider other )
	{
	}
}
