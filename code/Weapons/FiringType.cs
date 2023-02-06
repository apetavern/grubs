namespace Grubs.Weapons.Base;

/// <summary>
/// Defines the way that a weapon will be fired.
/// </summary>
public enum FiringType
{
	/// <summary>
	/// Fires as soon as you press the fire button.
	/// </summary>
	Instant,
	/// <summary>
	/// Weapon can be charged and fired upon releasing the fire button.
	/// </summary>
	Charged
}
