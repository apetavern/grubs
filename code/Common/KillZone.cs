using Grubs.Helpers;
using Grubs.Pawn;

namespace Grubs.Common;

[Title( "Grubs - Kill Zone" ), Category( "Grubs" )]
public class KillZone : Component, Component.ITriggerListener
{
	[Property] public ParticleSystem KillParticles { get; set; }
	[Property] public SoundEvent KillSound { get; set; }

	public void OnTriggerEnter( Collider other )
	{
		// kidd: Workaround for ArcProjectile being destroyed immediately for non-owner clients,
		// despite the Transform appearing to be fine. Probably an interp bug, but Transform.ClearInterpolation()
		// in ArcProjectile.OnStart() didn't do SHIT.
		if ( Connection.Local != other.GameObject.Root.Network.OwnerConnection )
			return;

		if ( other.Transform.Position == 0f )
			return;

		CollisionEffects( other.Transform.World );

		if ( other.GameObject.Components.TryGet( out Grub grub, FindMode.EverythingInSelfAndAncestors ) )
		{
			grub.Health.TakeDamage( GrubsDamageInfo.FromKillZone(), true );
		}
		else
		{
			DestroyObjectWithTags( other.GameObject, "projectile", "drop" );
		}
	}

	private void DestroyObjectWithTags( GameObject go, params string[] args )
	{
		foreach ( var tag in args )
		{
			if ( go.Tags.Has( "ignore_killzone" ) )
				continue;

			if ( go.Tags.Has( tag ) && go.Transform.Position != 0f )
			{
				CollisionEffects( go.Transform.World );
				go.Destroy();
			}
		}
	}

	public void OnTriggerExit( Collider other )
	{
	}

	[Broadcast]
	public void CollisionEffects( Transform transform )
	{
		if ( KillSound is not null )
			Sound.Play( KillSound, transform.Position );

		if ( KillParticles is not null )
			ParticleHelper.Instance.PlayInstantaneous( KillParticles, transform );
	}
}
