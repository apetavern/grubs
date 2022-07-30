namespace Grubs.Player;

public partial class WormController : BasePlayerController
{
	[Net] public float Gravity { get; set; } = 800.0f;
	[Net] public bool IsGrounded { get; private set; }

	public override void Simulate()
	{
		TouchGrass();
	}

	// Temporary grounding method.
	private void TouchGrass()
	{
		var tr = Trace.Ray( Position, Position + (Vector3.Down * 10000) )
			.WorldOnly()
			.Run();

		if ( tr.Hit )
		{
			Position = tr.EndPosition;
			IsGrounded = true;
		}
	}
}
