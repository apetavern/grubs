using Grubs.Terrain;

namespace Grubs.Utils.Extensions;

/// <summary>
/// Extension methods for <see cref="DamageInfo"/>.
/// </summary>
public static class DamageInfoExtension
{
	/// <summary>
	/// Creates a <see cref="DamageInfo"/> from an explosion.
	/// </summary>
	/// <param name="damage">The amount of damage to deal.</param>
	/// <param name="position">The position of the explosion.</param>
	/// <param name="force">The force of the explosion.</param>
	/// <param name="attacker">The <see cref="Entity"/> that caused the explosion.</param>
	/// <returns>The constructed <see cref="DamageInfo"/>.</returns>
	public static DamageInfo FromExplosion( float damage, Vector3 position, Vector3 force, Entity attacker )
	{
		return new DamageInfo
		{
			Tags = { "explosive" },
			Position = position,
			Damage = damage,
			Attacker = attacker,
			Force = force,
		};
	}

	/// <summary>
	/// Creates a <see cref="DamageInfo"/> from a fall.
	/// </summary>
	/// <param name="damage">The amount of fall damage to receive.</param>
	/// <param name="attacker">The <see cref="Entity"/> that caused the fall damage.</param>
	/// <returns>The constructed <see cref="DamageInfo"/>.</returns>
	public static DamageInfo FromFall( float damage, Entity attacker )
	{
		return new DamageInfo
		{
			Tags = { "fall" },
			Damage = damage,
			Attacker = attacker
		};
	}

	/// <summary>
	/// Creates a <see cref="DamageInfo"/> from a <see cref="DamageZone"/>s information.
	/// </summary>
	/// <param name="zone">The <see cref="DamageZone"/> to take information from.</param>
	/// <returns>The constructed <see cref="DamageInfo"/>.</returns>
	public static DamageInfo FromZone( DamageZone zone )
	{
		return new DamageInfo
		{
			Tags = zone.DamageTags,
			Damage = zone.DamagePerTrigger
		};
	}
}
