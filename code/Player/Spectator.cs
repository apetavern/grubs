namespace Grubs.Player;

/// <summary>
/// A simple pawn for spectators to control.
/// </summary>
[Category( "Spectators" )]
public sealed partial class Spectator : Entity, ISpectator
{
	[Net]
	public Entity Camera { get; private set; }

	public Spectator()
	{
		Camera = new GrubsCamera()
		{
			Owner = this
		};
	}
}
