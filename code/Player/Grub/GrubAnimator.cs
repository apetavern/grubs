using Grubs.Utils;

namespace Grubs.Player;

public class GrubAnimator : PawnAnimator
{
	private float _incline;

	public override void Simulate()
	{
		var grub = Pawn as Grub;
		var controller = grub!.Controller;

		SetAnimParameter( "grounded", controller.IsGrounded );
		SetAnimParameter( "hardfall", controller.IsHardFalling );
		SetAnimParameter( "lowhp", grub.Health < GameConfig.LowHealthThreshold );
		SetAnimParameter( "explode", grub.LifeState == LifeState.Dying );
		SetAnimParameter( "sliding", grub.HasBeenDamaged && !controller.IsHardFalling && !controller.Velocity.IsNearlyZero( 2.5f ) );

		var velocity = WishVelocity.Length;// Pawn.Velocity.Cross( Vector3.Up ).Length;
		SetAnimParameter( "velocity", velocity );

		var aimAngle = -Pawn.EyeRotation.Pitch().Clamp( -80f, 75f );
		SetAnimParameter( "aimangle", aimAngle );

		var tr = Trace.Ray( Pawn.Position + Pawn.Rotation.Up * 10f, Pawn.Position + Pawn.Rotation.Down * 128 )
			.Ignore( Pawn )
			.IncludeClientside()
			.Run();
		_incline = MathX.Lerp( _incline, Pawn.Rotation.Forward.Angle( tr.Normal ) - 90f, 0.25f );

		SetAnimParameter( "incline", _incline );
		SetAnimParameter( "heightdiff", tr.Distance );
	}
}
