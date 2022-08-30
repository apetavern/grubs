namespace Grubs.Player;

/// <summary>
/// Encapsulates all the ways a grub can be damaged.
/// </summary>
public enum GrubDamageType
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
	/// Falling from a great height.
	/// </summary>
	Fall,
	/// <summary>
	/// Touching an instance kill zone.
	/// </summary>
	KillTrigger
}
