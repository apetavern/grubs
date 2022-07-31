namespace Grubs.Player;

public class WormAnimator : PawnAnimator
{
	public override void Simulate()
	{
		var controller = (Pawn as Worm).Controller;

		SetAnimParameter( "grounded", controller.IsGrounded );

		float velocity = Pawn.Velocity.Cross( Vector3.Up ).Length;
		SetAnimParameter( "velocity", velocity );
	}
}
