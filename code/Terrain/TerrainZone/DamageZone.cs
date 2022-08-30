using Grubs.Player;
using Grubs.Utils.Extensions;
using Grubs.Weapons.Projectiles;

namespace Grubs.Terrain;

/// <summary>
/// Handles a zone that can deal damage to entities.
/// </summary>
public partial class DamageZone : TerrainZone
{
	/// <summary>
	/// The flags to pass to the damage info when a grub is damaged.
	/// </summary>
	[Net]
	public DamageFlags DamageFlags { get; private set; }

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
	/// Sets the damage flags to use in the damage applied to the entity.
	/// </summary>
	/// <param name="flags">The flags to set.</param>
	/// <param name="add">Whether the flags are added or stomped.</param>
	/// <returns>The damage zone instance.</returns>
	public DamageZone WithDamageFlags( DamageFlags flags, bool add = true )
	{
		if ( add )
			DamageFlags |= flags;
		else
			DamageFlags = flags;

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

		if ( entity is Projectile projectile )
		{
			projectile.Delete();
			return;
		}

		if ( entity is not Grub grub )
			return;

		var damageInfo = DamageInfoExtension.FromZone( this );
		damageInfo.Position = grub.Position;
		grub.TakeDamage( damageInfo );
	}
}
