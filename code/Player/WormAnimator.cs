namespace Grubs.Player;

public class WormAnimator : PawnAnimator
{
	public override void Simulate()
	{
		var controller = (Pawn as Worm).Controller;

		SetAnimParameter( "grounded", controller.IsGrounded );

		float velocity = Pawn.Velocity.Cross( Vector3.Up ).Length;
		SetAnimParameter( "velocity", velocity );

		var tr = Trace.Ray( Pawn.Position, Pawn.Position + Pawn.Rotation.Down * 128 ).Ignore( Pawn ).Run();
		float incline = Pawn.Rotation.Forward.Angle( tr.Normal ) - 90f;
		SetAnimParameter( "incline", incline );
	}
}
