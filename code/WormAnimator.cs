using Sandbox;
using System;
using TerryForm.Weapons;

namespace TerryForm
{
	public class WormAnimator : PawnAnimator
	{
		public override void Simulate()
		{
			DoRotation();

			// Aim angle
			{
				float aimAngle = -Pawn.EyeRot.Pitch().Clamp( -80f, 35f );
				SetParam( "aimangle", aimAngle );
			}

			{
				// Trace down to ground, then work out the angle based on where the player's facing
				var tr = Trace.Ray( Pawn.Position, Pawn.Position + Pawn.Rotation.Down * 128 ).WorldOnly().Ignore( Pawn ).Run();
				float incline = Pawn.Rotation.Forward.Angle( tr.Normal ) - 90f;

				// TODO: How do we handle offsetting the player's model from their bbox?
				SetParam( "incline", incline );
			}

			// Velocity
			{
				float velocity = Pawn.Velocity.Cross( Vector3.Up ).Length.LerpInverse( 0f, 100f );
				SetParam( "velocity", velocity );
			}

			// Hold pose (can we get away with setting this in the weapon itself?)
			{
				if ( Pawn is Player { EquippedWeapon: Weapon weapon } )
				{
					SetParam( "holdpose", (int)weapon.HoldPose );
				}
				else
				{
					SetParam( "holdpose", (int)HoldPose.None );
				}
			}
		}

		private void DoRotation()
		{
			//
			// Rotate the player when we try to look backwards
			//
			float playerFacing = Pawn.EyeRot.Forward.Dot( Pawn.Rotation.Forward );
			if ( playerFacing < 0 )
			{
				Rotation *= Rotation.From( 0, 180, 0 ); // Super janky
				Pawn.ResetInterpolation();
			}
		}
	}
}
