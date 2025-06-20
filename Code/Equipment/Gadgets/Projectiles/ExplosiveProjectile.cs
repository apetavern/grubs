﻿using Grubs.Common;
using Grubs.Helpers;
using Grubs.Systems.Particles;
using Grubs.UI;

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
	[Property] public bool UseExplosionTimer { get; set; } = false;
	[Property, ResourceType( "sound" )] private string ExplosionSound { get; set; } = "";
	[Property, ResourceType( "vpcf" )] private ParticleSystem Particles { get; set; }
	[Property, ResourceType( "vpcf" )] private ParticleSystem SmokeParticles { get; set; }


	[Sync] private TimeUntil TimeUntilExplosion { get; set; }
	private TimeSince _timeSinceCreated = 0f;

	private ExplosiveTimer _explosiveTimerPanel;

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
		if ( UseExplosionTimer )
		{
			var prefab = ResourceLibrary.Get<PrefabFile>( "prefabs/world/explosivetimer.prefab" );
			var panel = SceneUtility.GetPrefabScene( prefab ).Clone();

			_explosiveTimerPanel = panel.Components.Get<ExplosiveTimer>();
			_explosiveTimerPanel.Target = GameObject;
			_explosiveTimerPanel.ExplodeAfter = seconds;
		}

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
		ExplodeEffects( WorldPosition, projectile?.GrubGuid ?? Guid.Empty, projectile?.GrubName ?? string.Empty );

		ProjectileExploded?.Invoke();

		if ( DeleteOnExplode )
			GameObject.Destroy();
	}

	[Rpc.Broadcast]
	public void ExplodeEffects( Vector3 position, Guid attackerGuid, string attackerName )
	{
		if ( ExplosionHelper.Instance.IsValid() )
			ExplosionHelper.Instance.Explode( this, position, ExplosionRadius, ExplosionDamage, attackerGuid, attackerName );
		
		if ( ExplosionSound is not null )
			Sound.Play( ExplosionSound, position );

		ExplosionParticles.Spawn()
			.SetWorldPosition( position )
			.SetScale( ExplosionRadius );
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		if ( _explosiveTimerPanel.IsValid() )
			_explosiveTimerPanel.GameObject.Destroy();
	}
}
