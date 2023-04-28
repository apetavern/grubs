namespace Grubs;

/// <summary>
/// Handles a zone that can deal damage to entities.
/// </summary>
public sealed partial class DamageZone : TerrainZone
{
	/// <summary>
	/// The flags to pass to the damage info when a grub is damaged.
	/// </summary>
	[Net]
	public IList<string> DamageTags { get; private set; } = null!;

	/// <summary>
	/// The damage that will be applied when another entity touches it.
	/// </summary>
	[Net]
	public float DamageOnTouch { get; private set; }

	/// <summary>
	/// The sound that is played when an entity touches the zone.
	/// </summary>
	public string TouchSound { get; private set; }

	/// <summary>
	/// The particle that is created when an entity touches the zone.
	/// </summary>
	public string ParticlePath { get; private set; }

	/// <summary>
	/// Sets the damage tags to use in the damage applied to the entity.
	/// </summary>
	/// <param name="tags">The tags to set.</param>
	/// <returns>The damage zone instance.</returns>
	public DamageZone WithDamageTags( params string[] tags )
	{
		foreach ( var tag in tags )
			DamageTags.Add( tag );
		return this;
	}

	/// <summary>
	/// Sets the amount of damage this zone will deal to anything that enters it.
	/// </summary>
	/// <param name="damage">The damage to deal.</param>
	/// <returns>The damage zone instance.</returns>
	public DamageZone WithDamage( float damage )
	{
		DamageOnTouch = damage;
		return this;
	}

	/// <summary>
	/// The sound to play when an entity touches the damage zone.
	/// </summary>
	/// <param name="sound">The sound string to play.</param>
	/// <returns>The damage zone instance.</returns>
	public DamageZone WithSound( string sound )
	{
		TouchSound = sound;
		return this;
	}

	/// <summary>
	/// The particles to create when an entity touches the damage zone.
	/// </summary>
	/// <param name="particlePath">The particles to create.</param>
	/// <returns>The damage zone instance.</returns>
	public DamageZone WithParticle( string particlePath )
	{
		ParticlePath = particlePath;
		return this;
	}

	public override void StartTouch( Entity entity )
	{
		if ( entity.Tags.Has( "preview" ) )
			return;

		var damageInfo = DamageInfoExtension.FromZone( this );
		damageInfo.Position = entity.Position;
		entity.TakeDamage( damageInfo );

		OnTouchSound( TouchSound );

		if ( !string.IsNullOrEmpty( ParticlePath ) )
			Particles.Create( ParticlePath, entity.Position.WithZ( CollisionBounds.Maxs.z ) );

		// Immediately apply damage given by a damage zone if it's a Grub.
		if ( entity is Grub grub )
			grub.ApplyDamage( damageInfo );
	}

	[ClientRpc]
	private void OnTouchSound( string sound )
	{
		this.SoundFromScreen( sound );
	}
}
