﻿namespace Grubs.Common;

/// <summary>
/// Encapsulates all the ways a grub can be damaged.
/// </summary>
public enum DamageType
{
	/// <summary>
	/// Just nothing.
	/// </summary>
	None,
	/// <summary>
	/// An explosion.
	/// </summary>
	Explosion,
	/// <summary>
	/// A fire.
	/// </summary>
	Fire,
	/// <summary>
	/// Falling from a great height.
	/// </summary>
	Fall,
	/// <summary>
	/// Shot by a HitScan weapon.
	/// </summary>
	HitScan,
	/// <summary>
	/// Touching an instant kill zone.
	/// </summary>
	KillZone,
	/// <summary>
	/// Killed by a melee weapon.
	/// </summary>
	Melee,
	/// <summary>
	/// Admin abuse.
	/// </summary>
	Admin,
	/// <summary>
	/// The player disconnected.
	/// </summary>
	Disconnect
}
