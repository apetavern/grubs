using Grubs.Common;
using Grubs.Helpers;
using Grubs.Pawn;

namespace Grubs.Equipment.Gadgets.Projectiles;

[Title( "Grubs - Explosive Projectile" ), Category( "Equipment" )]
public class ExplosiveProjectile : Component, IResolvable, Component.ICollisionListener
{
	[Property] private float ExplosionDamage { get; set; } = 50f;
	[Property] public float ExplosionRadius { get; set; } = 100f;
	[Property] public bool ExplodeOnCollision { get; set; } = false;
	[Property] public float CollisionDelay { get; set; } = 0f; // Delay before collision effects are applied
	[Property] public bool DeleteOnExplode { get; set; } = true;
	[Property] public bool ExplodeOnDeath { get; set; } = true;
	[Property, Sync] public float ExplodeAfter { get; set; } = 4.0f;
	[Property, ResourceType( "sound" )] private string ExplosionSound { get; set; } = "";
	[Property, ResourceType( "vpcf" )] private ParticleSystem Particles { get; set; }


	[Sync] private TimeUntil TimeUntilExplosion { get; set; }
	private TimeSince _timeSinceCreated = 0f;

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
		if ( !ExplodeOnCollision )
			return;

		if ( _timeSinceCreated < CollisionDelay )
			return;

		Explode();
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

		if ( !GameObject.IsValid() )
			return;

		Explode();
	}

	public void Explode()
	{
		if ( IsProxy )
			return;

		var projectile = Components.Get<Projectile>();
		ExplodeEffects( projectile?.GrubGuid ?? Guid.Empty, projectile?.GrubName ?? string.Empty );

		ProjectileExploded?.Invoke();

		if ( DeleteOnExplode )
			GameObject.Destroy();
	}

	[Broadcast]
	public void ExplodeEffects( Guid attackerGuid, string attackerName )
	{
		ExplosionHelper.Instance.Explode( this, Transform.Position, ExplosionRadius, ExplosionDamage, attackerGuid, attackerName );
		Sound.Play( ExplosionSound );

		if ( Particles is null )
			return;

		var sceneParticles = ParticleHelper.Instance.PlayInstantaneous( Particles, Transform.World );
		sceneParticles.SetControlPoint( 1, new Vector3( ExplosionRadius / 2f, 0, 0 ) );
	}
}
