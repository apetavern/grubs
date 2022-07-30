namespace Grubs.Player;

public class WormAnimator : PawnAnimator
{
	public override void Simulate()
	{
		var controller = (Pawn as Worm).Controller;

		SetAnimParameter( "grounded", controller.IsGrounded );
	}
}
