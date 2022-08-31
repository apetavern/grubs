namespace Grubs.Player;

public class GrubAnimator : PawnAnimator
{
	public override void Simulate()
	{
		var grub = Pawn as Grub;
		var controller = grub!.Controller;

		SetAnimParameter( "grounded", controller.IsGrounded );
		SetAnimParameter( "hardfall", controller.IsHardFalling );
		// TODO: Make this not hard coded
		SetAnimParameter( "lowhp", grub.Health < 30 );
		SetAnimParameter( "explode", grub.LifeState == LifeState.Dying );

		float velocity = Pawn.Velocity.Cross( Vector3.Up ).Length;
		SetAnimParameter( "velocity", velocity );

		float aimAngle = -Pawn.EyeRotation.Pitch().Clamp( -80f, 75f );
		SetAnimParameter( "aimangle", aimAngle );

		var tr = Trace.Ray( Pawn.Position, Pawn.Position + Pawn.Rotation.Down * 128 ).Ignore( Pawn ).IncludeClientside().Run();
		float incline = Pawn.Rotation.Forward.Angle( tr.Normal ) - 90f;
		SetAnimParameter( "incline", incline );
	}
}
