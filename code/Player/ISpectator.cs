namespace Grubs.Player;

/// <summary>
/// Defines something that has a camera in the game world.
/// </summary>
public interface ISpectator
{
	CameraMode Camera { get; }
}
