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
	/// Whether or not this zone is a kill barrier.
	/// </summary>
	[Net]
	public bool InstantKill { get; private set; }

	/// <summary>
	/// The damage for every turn the grub is in it.
	/// <remarks>In the case of <see cref="InstantKill"/> being true. This will be the damage applied to kill the grub.</remarks>
	/// </summary>
	[Net]
	public float DamagePerTrigger { get; private set; }

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
	/// Sets whether or not this zone is meant to instant kill anything that enters it.
	/// </summary>
	/// <param name="instantKill">Whether or not to instant kill.</param>
	/// <returns>The damage zone instance.</returns>
	public DamageZone WithInstantKill( bool instantKill )
	{
		InstantKill = instantKill;
		return this;
	}

	/// <summary>
	/// Sets the amount of damage this zone will deal to anything that enters it.
	/// </summary>
	/// <param name="damage">The damage to deal.</param>
	/// <returns>The damage zone instance.</returns>
	public DamageZone WithDamage( float damage )
	{
		DamagePerTrigger = damage;
		return this;
	}

	/// <summary>
	/// Deals damage to an entity that is inside the zone.
	/// </summary>
	/// <param name="entity">The entity that is being damaged.</param>
	public override void Trigger( Entity entity )
	{
		base.Trigger( entity );

		if ( !InZone( entity ) )
			return;

		var damageInfo = DamageInfoExtension.FromZone( this );
		damageInfo.Position = entity.Position;
		entity.TakeDamage( damageInfo );
	}
}
