namespace Grubs.Player;

public class GrubAnimator : PawnAnimator
{
	public override void Simulate()
	{
		var controller = (Pawn as Grub)!.Controller;

		SetAnimParameter( "grounded", controller.IsGrounded );

		float velocity = Pawn.Velocity.Cross( Vector3.Up ).Length;
		SetAnimParameter( "velocity", velocity );

		float aimAngle = -Pawn.EyeRotation.Pitch().Clamp( -80f, 75f );
		SetAnimParameter( "aimangle", aimAngle );

		var tr = Trace.Ray( Pawn.Position, Pawn.Position + Pawn.Rotation.Down * 128 ).Ignore( Pawn ).IncludeClientside().Run();
		float incline = Pawn.Rotation.Forward.Angle( tr.Normal ) - 90f;
		SetAnimParameter( "incline", incline );
	}
}
