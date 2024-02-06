using Grubs.Helpers;

namespace Grubs.Equipment.Weapons;

[Title( "Grub - Explosive Projectile" ), Category( "Equipment" )]
public class ExplosiveProjectileComponent : Component
{
	[Property] private float ExplosionDamage { get; set; } = 50f;
	[Property] private float ExplosionRadius { get; set; } = 100f;
	[Property] public bool ExplodeOnCollision { get; set; } = false;
	[Property] public bool DeleteOnExplode { get; set; } = true;
	[Property, Sync] public float ExplodeAfter { get; set; } = 4.0f;
	[Property, ResourceType( "sound" )] private string ExplosionSound { get; set; } = "";
	[Property, ResourceType( "vpcf" )] private ParticleSystem? Particles { get; set; }

	[Net] private TimeUntil TimeUntilExplosion { get; set; }

	protected override void OnStart()
	{
		if ( ExplodeAfter > 0f )
		{
			TimeUntilExplosion = ExplodeAfter;
			ExplodeAfterSeconds( ExplodeAfter );
		}
	}

	private async void ExplodeAfterSeconds( float seconds )
	{
		await GameTask.DelaySeconds( seconds );

		if ( !GameObject.IsValid )
			return;

		Explode();
	}

	public void Explode()
	{
		if ( IsProxy )
			return;

		ExplodeEffects();
		ExplosionHelperComponent.Instance.Explode( this, Transform.Position, ExplosionRadius, ExplosionDamage );

		if ( DeleteOnExplode )
			GameObject.Destroy();
	}

	[Broadcast]
	public void ExplodeEffects()
	{
		if ( Particles is null )
			return;

		Sound.Play( ExplosionSound );
		var sceneParticles = ParticleHelperComponent.Instance.PlayInstantaneous( Particles, Transform.World );
		sceneParticles.SetControlPoint( 1, new Vector3( ExplosionRadius / 2f, 0, 0 ) );
	}
}
