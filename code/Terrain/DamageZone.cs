using Grubs.Player;
using Grubs.Utils.Extensions;
using Grubs.Weapons.Projectiles;

namespace Grubs.Terrain;

/// <summary>
/// Handles a zone in the terrain that can cause damage to grubs.
/// </summary>
public partial class DamageZone : BaseNetworkable
{
	/// <summary>
	/// Networked list of all damage zones in the terrain.
	/// </summary>
	[Net]
	public static IList<DamageZone> All { get; private set; } = new List<DamageZone>();

	/// <summary>
	/// The flags to pass to the damage info when a grub is damaged.
	/// </summary>
	public virtual DamageFlags DamageFlags => DamageFlags.Direct;
	/// <summary>
	/// Whether or not this zone is a kill barrier.
	/// </summary>
	public virtual bool InstantKill => true;
	/// <summary>
	/// The damage for every turn the grub is in it.
	/// <remarks>In the case of <see cref="InstantKill"/> being true. This will be the damage applied to kill the grub.</remarks>
	/// </summary>
	public virtual float DamagePerTurn => 9999;

	/// <summary>
	/// The position of the zone.
	/// </summary>
	[Net]
	public Vector3 Position { get; set; }
	/// <summary>
	/// The size of the zone.
	/// </summary>
	[Net]
	public Vector3 Size { get; set; }

	/// <summary>
	/// Returns whether or not an entity is inside this zone.
	/// </summary>
	/// <param name="entity">The entity position to check.</param>
	/// <returns>Whether or not the entity is inside this zone.</returns>
	public virtual bool InZone( Entity entity )
	{
		var mins = Vector3.Min( Position, Position + Size );
		var maxs = Vector3.Max( Position, Position + Size );

		var position = entity.Position;
		return position.x >= mins.x && position.x <= maxs.x &&
			   position.y >= mins.y && position.y <= maxs.y &&
			   position.z >= mins.z && position.z <= maxs.z;
	}

	/// <summary>
	/// Deals damage to an entity that is inside the zone.
	/// </summary>
	/// <param name="entity">The entity that is being damaged.</param>
	/// <param name="immediate">Whether or not this entity needs to receive the damage right now.</param>
	public virtual void DealDamage( Entity entity, bool immediate )
	{
		Host.AssertServer();

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

		if ( immediate )
			grub.ApplyDamage();
	}

	/// <summary>
	/// Debug console variable to see the kill zones area.
	/// </summary>
	[ConVar.Server( "kz_debug" )]
	public static bool KillZoneDebug { get; set; }

	/// <summary>
	/// Shows all the kill zones if <see cref="KillZoneDebug"/> is true.
	/// </summary>
	[Event.Tick.Server]
	public static void DebugKillZones()
	{
		if ( !KillZoneDebug )
			return;

		foreach ( var zone in All )
			DebugOverlay.Box( zone.Position, zone.Position + zone.Size, Color.Gray, 1 );
	}
}
