using Grubs.Utils;

namespace Grubs.Player;

partial class Grub
{
	private float _incline;

	private void SimulateAnimation( IClient cl )
	{
		var ctrl = Controller;

		/*		SetAnimParameter( "grounded", Controller.IsGrounded );
				SetAnimParameter( "hardfall", Controller.IsHardFalling );
				SetAnimParameter( "lowhp", Health < GameConfig.LowHealthThreshold );
				SetAnimParameter( "explode", LifeState == LifeState.Dying );
				SetAnimParameter( "sliding", HasBeenDamaged && !Controller.IsHardFalling && !Controller.Velocity.IsNearlyZero( 2.5f ) );

				var velocity = ctrl.WishVelocity.Length;// Pawn.Velocity.Cross( Vector3.Up ).Length;
				SetAnimParameter( "velocity", velocity );

				var aimAngle = -EyeRotation.Pitch().Clamp( -80f, 75f );
				SetAnimParameter( "aimangle", aimAngle );

				var tr = Trace.Ray( Position + Rotation.Up * 10f, Position + Rotation.Down * 128 )
					.Ignore( this )
					.IncludeClientside()
					.Run();
				_incline = MathX.Lerp( _incline, Rotation.Forward.Angle( tr.Normal ) - 90f, 0.25f );

				SetAnimParameter( "incline", _incline );
				SetAnimParameter( "heightdiff", tr.Distance );*/
	}
}
