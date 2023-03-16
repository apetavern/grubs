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

	public override void StartTouch( Entity entity )
	{
		var damageInfo = DamageInfoExtension.FromZone( this );
		damageInfo.Position = entity.Position;
		entity.TakeDamage( damageInfo );

		// Immediately apply damage given by a damage zone if it's a Grub.
		if ( entity is Grub grub )
			grub.ApplyDamage();
	}
}
