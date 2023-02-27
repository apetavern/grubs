namespace Grubs;

[Category( "Weapon" )]
public partial class Explosive : ModelEntity
{
	private Grub Grub { get; set; }
	private float ExplosionRadius { get; set; } = 1000;
	private string ExplosionSound { get; set; } = "";
	private string TrailParticle { get; set; } = "";
	private ExplosiveReaction CollisionReaction { get; set; }

	public Explosive()
	{
		Transmit = TransmitType.Always;
	}

	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Always;
	}

	/// <summary>
	/// Sets the grub that is the reason for this explosive existing.
	/// </summary>
	/// <param name="grub">The grub that created this explosive.</param>
	/// <returns>The explosive instance.</returns>
	public Explosive WithGrub( Grub grub )
	{
		Grub = grub;
		return this;
	}

	/// <summary>
	/// Sets the model of this explosive to the passed in path.
	/// </summary>
	/// <param name="modelPath">The path to the model to set.</param>
	/// <returns>The explosive instance.</returns>
	public Explosive WithModel( string modelPath )
	{
		SetModel( modelPath );
		return this;
	}

	/// <summary>
	/// Sets the model of this explosive to the passed in model.
	/// </summary>
	/// <param name="model">The model to be set.</param>
	/// <returns>The explosive instance.</returns>
	public Explosive WithModel( Model model )
	{
		Model = model;
		return this;
	}

	/// <summary>
	/// Sets the position of this explosive.
	/// </summary>
	/// <param name="position">The position of the explosive.</param>
	/// <returns>The explosive instance.</returns>
	public Explosive WithPosition( Vector3 position )
	{
		Position = position.WithY( 0f );
		return this;
	}

	/// <summary>
	/// Sets the radius of the explosion it creates when it explodes.
	/// </summary>
	/// <param name="explosionRadius">The radius of the explosion.</param>
	/// <returns>The explosive instance.</returns>
	public Explosive WithExplosionRadius( float explosionRadius )
	{
		ExplosionRadius = explosionRadius;
		return this;
	}

	/// <summary>
	/// Sets the sound that plays when the explosive explodes.
	/// </summary>
	/// <param name="explosionSound">The explosion sound path.</param>
	/// <returns>The explosive instance.</returns>
	public Explosive WithExplosionSound( string explosionSound )
	{
		ExplosionSound = explosionSound;
		return this;
	}

	/// <summary>
	/// What happens when the explosive "collides".
	/// </summary>
	/// <param name="reaction"></param>
	/// <returns>The explosive instance.</returns>
	public Explosive SetCollisionReaction( ExplosiveReaction reaction )
	{
		CollisionReaction = reaction;
		return this;
	}

	/// <summary>
	/// Sets the particle used for the trail of the explosive.
	/// </summary>
	/// <param name="particlePath">The particle path.</param>
	/// <returns>The explosive instance.</returns>
	public Explosive WithTrailParticle( string particlePath )
	{
		TrailParticle = particlePath;

		if ( !string.IsNullOrEmpty( TrailParticle ) )
			Particles.Create( TrailParticle, this, "trail" );

		return this;
	}

	/// <summary>
	/// Verifies that the explosive has its basic information set.
	/// </summary>
	/// <returns>The explosive instance.</returns>
	public Explosive Finish()
	{
		Health = 1;
		return this;
	}

	/// <summary>
	/// Explodes the explosive after an amount of seconds.
	/// </summary>
	/// <param name="seconds">The amount of seconds to wait before exploding.</param>
	public async void ExplodeAfterSeconds( float seconds )
	{
		await GameTask.DelaySeconds( seconds );

		if ( !IsValid )
			return;

		Explode();
	}

	public override void Simulate( IClient client )
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

	protected void Explode()
	{
		switch ( CollisionReaction )
		{
			case ExplosiveReaction.Explosion:
				ExplosionHelper.Explode( Position, Grub, ExplosionRadius );
				break;
			case ExplosiveReaction.Incendiary:
				// FireHelper.StartFiresAt( Position, Segments[Segments.Count - 1].EndPos - Segments[Segments.Count - 1].StartPos, 10 );
				break;
		}

		ExplodeSoundClient( To.Everyone, ExplosionSound );
		Delete();
	}

	[ClientRpc]
	private void ExplodeSoundClient( string explosionSound )
	{
		this.SoundFromScreen( explosionSound );
	}
}

/// <summary>
/// Defines the type of reaction a <see cref="Explosion"/> has when it collides.
/// </summary>
public enum ExplosiveReaction
{
	/// <summary>
	/// Produces a regular explosion.
	/// </summary>
	Explosion,
	/// <summary>
	/// Produces a fire.
	/// </summary>
	Incendiary
}
