using Sandbox;
using TerryForm.Weapons;

namespace TerryForm.Pawn
{
	public class WormAnimator : PawnAnimator
	{
		public override void Simulate()
		{
			DoRotation();

			// Aim angle
			{
				float aimAngle = -Pawn.EyeRot.Pitch().Clamp( -80f, 75f );
				SetParam( "aimangle", aimAngle );
			}

			// Calculate incline
			{
				// Trace down to ground, then work out the angle based on where the player's facing
				var tr = Trace.Ray( Pawn.Position, Pawn.Position + Pawn.Rotation.Down * 128 ).WorldOnly().Ignore( Pawn ).Run();
				float incline = Pawn.Rotation.Forward.Angle( tr.Normal ) - 90f;

				// TODO: How do we handle offsetting the player's model from their bbox?
				SetParam( "incline", incline );
			}

			// Grounded check
			{
				SetParam( "grounded", Pawn.GroundEntity != null );
			}

			// Velocity
			{
				float velocity = Pawn.Velocity.Cross( Vector3.Up ).Length.LerpInverse( 0f, 100f );
				SetParam( "velocity", velocity );
			}

			// Hold pose (can we get away with setting this in the weapon itself?)
			{
				if ( Pawn is Worm { EquippedWeapon: Weapon weapon } )
				{
					SetParam( "holdpose", (int)weapon.HoldPose );
				}
				else
				{
					SetParam( "holdpose", (int)HoldPose.None );
				}
			}
		}

		/// <summary>
		/// Rotate the player when they try to look backwards
		/// </summary>
		private void DoRotation()
		{
			float playerFacing = Pawn.EyeRot.Forward.Dot( Pawn.Rotation.Forward );
			if ( playerFacing < 0 )
			{
				Rotation *= Rotation.From( 0, 180, 0 ); // Super janky
				Pawn.ResetInterpolation();
			}
		}

		public override void OnEvent( string name )
		{
			base.OnEvent( name );
			Trigger( name );
		}
	}
}
