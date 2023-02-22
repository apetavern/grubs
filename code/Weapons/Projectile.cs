﻿namespace Grubs;

[Category( "Weapon" )]
public partial class Projectile : ModelEntity
{
	[Net]
	public bool ShouldFollowProjectile { get; private set; } = true;

	public bool HasBeenDamaged => false;
	public bool Resolved => false;

	private Grub Grub { get; set; }
	private float Speed { get; set; } = 0.001f;
	private float ExplosionRadius { get; set; } = 1000;
	private float CollisionExplosionDelaySeconds { get; set; }
	private List<ArcSegment> Segments { get; set; } = new();
	private string ExplosionSound { get; set; } = "";
	private string TrailParticle { get; set; } = "";
	private ProjectileCollisionReaction CollisionReaction { get; set; }

	public Projectile()
	{
		Transmit = TransmitType.Always;
	}

	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Always;
	}

	/// <summary>
	/// Sets the grub that is the reason for this projectile existing.
	/// </summary>
	/// <param name="grub">The grub that created this projectile.</param>
	/// <returns>The projectile instance.</returns>
	public Projectile WithGrub( Grub grub )
	{
		Grub = grub;
		return this;
	}

	/// <summary>
	/// Sets the model of this projectile to the passed in path.
	/// </summary>
	/// <param name="modelPath">The path to the model to set.</param>
	/// <returns>The projectile instance.</returns>
	public Projectile WithModel( string modelPath )
	{
		SetModel( modelPath );
		return this;
	}

	/// <summary>
	/// Sets the model of this projectile to the passed in model.
	/// </summary>
	/// <param name="model">The model to be set.</param>
	/// <returns>The projectile instance.</returns>
	public Projectile WithModel( Model model )
	{
		Model = model;
		return this;
	}

	/// <summary>
	/// Sets the position of this projectile.
	/// </summary>
	/// <param name="position">The position of the projectile.</param>
	/// <returns>The projectile instance.</returns>
	public Projectile WithPosition( Vector3 position )
	{
		Position = position.WithY( 0f );
		return this;
	}

	/// <summary>
	/// Sets the speed the projectile will move at.
	/// </summary>
	/// <param name="speed">The speed to move at.</param>
	/// <returns>The projectile instance.</returns>
	public Projectile WithSpeed( float speed )
	{
		Speed = 1 / speed;
		return this;
	}

	/// <summary>
	/// Sets the radius of the explosion it creates when it explodes.
	/// </summary>
	/// <param name="explosionRadius">The radius of the explosion.</param>
	/// <returns>The projectile instance.</returns>
	public Projectile WithExplosionRadius( float explosionRadius )
	{
		ExplosionRadius = explosionRadius;
		return this;
	}

	/// <summary>
	/// Sets the delay to which the projectile will explode after colliding.
	/// </summary>
	/// <param name="delaySeconds">The seconds to delay for.</param>
	/// <returns>The projectile instance.</returns>
	public Projectile WithCollisionExplosionDelay( float delaySeconds )
	{
		CollisionExplosionDelaySeconds = delaySeconds;
		return this;
	}

	/// <summary>
	/// Sets the sound that plays when the projectile explodes.
	/// </summary>
	/// <param name="explosionSound">The explosion sound path.</param>
	/// <returns>The projectile instance.</returns>
	public Projectile WithExplosionSound( string explosionSound )
	{
		ExplosionSound = explosionSound;
		return this;
	}

	/// <summary>
	/// What happens when the projectile "collides".
	/// </summary>
	/// <param name="reaction"></param>
	/// <returns>The projectile instance.</returns>
	public Projectile SetCollisionReaction( ProjectileCollisionReaction reaction )
	{
		CollisionReaction = reaction;
		return this;
	}

	/// <summary>
	/// Whether or not the camera should follow the projectile.
	/// </summary>
	/// <param name="following"></param>
	/// <returns>The projectile instance.</returns>
	public Projectile WithProjectileFollow( bool following )
	{
		ShouldFollowProjectile = following;
		return this;
	}

	/// <summary>
	/// Sets the particle used for the trail of the projectile.
	/// </summary>
	/// <param name="particlePath">The particle path.</param>
	/// <returns>The projectile instance.</returns>
	public Projectile WithTrailParticle( string particlePath )
	{
		TrailParticle = particlePath;

		if ( !string.IsNullOrEmpty( TrailParticle ) )
			Particles.Create( TrailParticle, this, "trail" );

		return this;
	}

	/// <summary>
	/// Sets a path for the projectile to follow.
	/// </summary>
	/// <param name="points">The arc trace segments to follow.</param>
	/// <returns>The projectile instance.</returns>
	public Projectile MoveAlongTrace( List<ArcSegment> points )
	{
		Segments = points;

		// Set the initial position
		Position = Segments[0].StartPos;

		return this;
	}

	/// <summary>
	/// Verifies that the projectile has its basic information set.
	/// </summary>
	/// <returns>The projectile instance.</returns>
	public Projectile Finish()
	{
		Health = 1;
		return this;
	}

	/// <summary>
	/// Explodes the projectile after an amount of seconds.
	/// </summary>
	/// <param name="seconds">The amount of seconds to wait before exploding.</param>
	public async void ExplodeAfterSeconds( float seconds )
	{
		await GameTask.DelaySeconds( seconds );

		if ( !IsValid )
			return;

		Explode();
	}

	public bool GiveHealth( float health )
	{
		Health += health;
		return true;
	}

	public bool ApplyDamage()
	{
		return Health <= 0;
	}

	[Event.Tick.Server]
	private void Tick()
	{
		if ( ProjectileDebug )
			DrawSegments();

		if ( Segments.Count > 0 )
			HandleSegmentTick();
		else
			HandlePhysicsTick();
	}

	private void HandleSegmentTick()
	{
		if ( (Segments[0].EndPos - Position).IsNearlyZero( 2.5f ) )
		{
			if ( Segments.Count > 1 )
				Segments.RemoveAt( 0 );
			else
				OnCollision();

			return;
		}

		Rotation = Rotation.LookAt( Segments[0].EndPos - Segments[0].StartPos );
		Position = Vector3.Lerp( Segments[0].StartPos, Segments[0].EndPos, Time.Delta / Speed );
	}

	private void HandlePhysicsTick()
	{
		// Apply gravity.
		Velocity -= new Vector3( 0, 0, 400 ) * Time.Delta;

		var helper = new MoveHelper( Position, Velocity );
		helper.Trace = helper.Trace.Size( 12f ).WithAnyTags( "player", "solid" ).WithoutTags( "dead" );
		helper.TryMove( Time.Delta );
		Velocity = helper.Velocity;
		Position = helper.Position;

		// Apply rotation using some shit I pulled out of my ass.
		var angularX = Velocity.x * 5f * Time.Delta;
		float degrees = angularX.Clamp( -20, 20 );
		Rotation = Rotation.RotateAroundAxis( new Vector3( 0, 1, 0 ), degrees );
	}

	private void OnCollision()
	{
		if ( !Game.IsServer )
			return;

		if ( CollisionExplosionDelaySeconds > 0 )
		{
			ExplodeAfterSeconds( CollisionExplosionDelaySeconds );
			return;
		}

		Explode();
	}

	private void Explode()
	{
		switch ( CollisionReaction )
		{
			case ProjectileCollisionReaction.Explosive:
				ExplosionHelper.Explode( Position, Grub, ExplosionRadius );
				break;
			case ProjectileCollisionReaction.Incendiary:
				// FireHelper.StartFiresAt( Position, Segments[Segments.Count - 1].EndPos - Segments[Segments.Count - 1].StartPos, 10 );
				break;
		}

		ExplodeSoundClient( To.Everyone, ExplosionSound );
		Delete();
	}

	[ClientRpc]
	public void ExplodeSoundClient( string explosionSound )
	{
		this.SoundFromScreen( explosionSound );
	}

	/// <summary>
	/// Debug console variable to see the projectiles path.
	/// </summary>
	[ConVar.Replicated( "projectile_debug" )]
	public static bool ProjectileDebug { get; set; }

	private void DrawSegments()
	{
		foreach ( var segment in Segments )
			DebugOverlay.Line( segment.StartPos, segment.EndPos );
	}
}

/// <summary>
/// Defines the type of reaction a <see cref="Projectile"/> has when it collides.
/// </summary>
public enum ProjectileCollisionReaction
{
	/// <summary>
	/// The projectile will explode.
	/// </summary>
	Explosive,
	/// <summary>
	/// The projectile will explode in fire.
	/// </summary>
	Incendiary
}
