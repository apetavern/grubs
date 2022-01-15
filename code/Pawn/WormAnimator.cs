using Sandbox;

namespace Grubs.Pawn
{
	public class WormAnimator : PawnAnimator
	{
		public override void Simulate()
		{
			var controller = (Pawn as Worm).Controller as WormController;

			// Grounded check
			{
				SetParam( "grounded", controller.IsGrounded );
			}

			// Sliding animation
			{
				//SetParam( "sliding", controller.IsSliding );
			}

			// Aim angle
			{
				float aimAngle = -Pawn.EyeRot.Pitch().Clamp( -80f, 75f );
				SetParam( "aimangle", aimAngle );
			}

			// Calculate incline
			{
				// Trace down to ground, then work out the angle based on where the player's facing
				var tr = Trace.Ray( Pawn.Position, Pawn.Position + Pawn.Rotation.Down * 128 ).Ignore( Pawn ).Run();
				float incline = Pawn.Rotation.Forward.Angle( tr.Normal ) - 90f;

				// TODO: How do we handle offsetting the player's model from their bbox?
				SetParam( "incline", incline );
			}

			// Velocity
			{
				float velocity = Pawn.Velocity.Cross( Vector3.Up ).Length;
				SetParam( "velocity", velocity );
			}
		}

		public override void OnEvent( string name )
		{
			base.OnEvent( name );
			Trigger( name );
		}
	}
}
