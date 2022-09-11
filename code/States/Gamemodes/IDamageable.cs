namespace Grubs.States;

/// <summary>
/// Defines something that can be damaged during gameplay.
/// </summary>
public interface IDamageable
{
	/// <summary>
	/// Makes the instance take damage.
	/// </summary>
	/// <param name="info">The damage to deal to the instance.</param>
	void TakeDamage( DamageInfo info );
	/// <summary>
	/// Gives health to the damageable instance.
	/// </summary>
	/// <param name="health">The amount of health to give.</param>
	/// <returns>Whether or not any health was healed.</returns>
	bool GiveHealth( float health );
	/// <summary>
	/// Applies any queued damage that may be in the instance.
	/// </summary>
	/// <returns>Whether or not the instance has been killed.</returns>
	bool ApplyDamage();
}
