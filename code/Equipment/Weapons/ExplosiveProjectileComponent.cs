using Grubs.Common;
using Grubs.Helpers;

namespace Grubs.Equipment.Weapons;

[Title( "Grubs - Explosive Projectile" ), Category( "Equipment" )]
public class ExplosiveProjectileComponent : Component, IResolvable, Component.ICollisionListener
{
	[Property] private float ExplosionDamage { get; set; } = 50f;
	[Property] private float ExplosionRadius { get; set; } = 100f;
	[Property] public bool ExplodeOnCollision { get; set; } = false;
	[Property] public bool DeleteOnExplode { get; set; } = true;
	[Property] public bool ExplodeOnDeath { get; set; } = true;
	[Property, Sync] public float ExplodeAfter { get; set; } = 4.0f;
	[Property, ResourceType( "sound" )] private string ExplosionSound { get; set; } = "";
	[Property, ResourceType( "vpcf" )] private ParticleSystem Particles { get; set; }

	[Sync] private TimeUntil TimeUntilExplosion { get; set; }

	public delegate void OnExplode();

	public event OnExplode ProjectileExploded;

	public virtual bool Resolved => TimeUntilExplosion;

	protected override void OnStart()
	{
		if ( ExplodeAfter > 0f )
		{
			TimeUntilExplosion = ExplodeAfter;
			ExplodeAfterSeconds( ExplodeAfter );
		}
	}

	public void OnCollisionStart( Collision other )
	{
		if ( ExplodeOnCollision )
		{
			Explode();
		}
	}

	public void OnCollisionUpdate( Collision other )
	{

	}

	public void OnCollisionStop( CollisionStop other )
	{

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

		ProjectileExploded?.Invoke();

		if ( DeleteOnExplode )
			GameObject.Destroy();
	}

	[Broadcast]
	public void ExplodeEffects()
	{
		ExplosionHelperComponent.Instance.Explode( this, Transform.Position, ExplosionRadius, ExplosionDamage );
		Sound.Play( ExplosionSound );

		if ( Particles is null )
			return;

		var sceneParticles = ParticleHelperComponent.Instance.PlayInstantaneous( Particles, Transform.World );
		sceneParticles.SetControlPoint( 1, new Vector3( ExplosionRadius / 2f, 0, 0 ) );
	}
}
