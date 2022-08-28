namespace Grubs.States;

/// <summary>
/// Defines something that must be resolved before a turn change can occur.
/// </summary>
public interface IResolvable
{
	/// <summary>
	/// Whether or not it has been resolved.
	/// </summary>
	bool Resolved { get; }
}
