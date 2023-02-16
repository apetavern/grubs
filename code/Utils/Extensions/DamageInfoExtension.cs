﻿namespace Grubs;

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
		return DamageInfo
			.FromExplosion( position, force, damage )
			.WithTag( "explosion" )
			.WithAttacker( attacker );
	}

	/// <summary>
	/// Creates a <see cref="DamageInfo"/> from a fall.
	/// </summary>
	/// <param name="damage">The amount of fall damage to receive.</param>
	/// <param name="attacker">The <see cref="Entity"/> that caused the fall damage.</param>
	/// <returns>The constructed <see cref="DamageInfo"/>.</returns>
	public static DamageInfo FromFall( float damage, Entity attacker )
	{
		return new DamageInfo()
			.WithTag( "fall" )
			.WithDamage( damage )
			.WithAttacker( attacker );
	}

	/// <summary>
	/// Creates a <see cref="DamageInfo"/> from a <see cref="DamageZone"/>s information.
	/// </summary>
	/// <param name="zone">The <see cref="DamageZone"/> to take information from.</param>
	/// <returns>The constructed <see cref="DamageInfo"/>.</returns>
	public static DamageInfo FromZone( DamageZone zone )
	{
		return new DamageInfo()
			.WithTags( zone.DamageTags.ToArray() )
			.WithDamage( zone.DamagePerTrigger );
	}
}
