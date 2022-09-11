namespace Grubs.Weapons.Base;

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
