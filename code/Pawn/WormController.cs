using Sandbox;
using System;
using Grubs.Utils;

namespace Grubs.Pawn
{
	public class WormController : BasePlayerController
	{
		// Controller settings
		public float Drag => 8.0f;
		public float AirDrag => 4.0f;
		public float Gravity => 800f;
		public float MaxSpeed => 60f;
		public float AirAcceleration => 600f;
		public float Acceleration => 810f;
		public float Step => 12f;
		public float Jump => 450f;

		// Jump properties
		private TimeSince TimeSinceJumpReleased { get; set; }
		private TimeSince TimeSinceJumped { get; set; }
		private bool HasJumpPending { get; set; }

		// Aim properties
		private Vector3 LookPos { get; set; }
		private float LookRotOffset { get; set; }

		// Movement properties
		private RealTimeUntil TimeUntilMovementAllowed { get; set; }
		private float FallStartPosZ { get; set; }
		public bool IsGrounded => GroundEntity != null;
		public bool IsSliding { get; set; }

		public override void Simulate()
		{
			var inputEnabled = (Pawn as Worm).IsCurrentTurn;

			if ( inputEnabled )
				SetEyeTransform();

			Move( inputEnabled );
		}

		private void Move( bool inputEnabled )
		{
			var mover = new MoveHelper( Position, Velocity );
			mover.Trace = mover.Trace.WorldAndEntities().Ignore( Pawn ).Size( 1.2f );
			mover.MaxStandableAngle = 45.0f;

			DoFriction( ref mover );

			// Gravity / set our ground entity
			CheckGroundEntity( ref mover );

			// Calculate movement speed
			var acceleration = IsGrounded ? Acceleration : AirAcceleration;

			// Calculate/add wish velocity
			Vector3 wishVelocity = default;

			if ( TimeUntilMovementAllowed <= 0 && inputEnabled )
				wishVelocity = -Input.Left * Vector3.Forward;

			wishVelocity = wishVelocity.Normal * acceleration * Time.Delta;

			// Limit the worms max speed.
			if ( Math.Abs( mover.Velocity.x ) < MaxSpeed )
				mover.Velocity += wishVelocity.WithZ( 0 );

			// Project our velocity onto the current surface we're stood on.
			var groundTrace = mover.TraceDirection( Vector3.Down );
			if ( groundTrace.Hit && groundTrace.Normal.Angle( Vector3.Up ) < mover.MaxStandableAngle && !IsSliding )
				mover.Velocity = ProjectOntoPlane( mover.Velocity, groundTrace.Normal );

			// Slide if surface normal exceeds max standable angle.
			DoSlide( ref mover, groundTrace );

			// Handle delayed jumping
			{
				// Schedule a jump.
				if ( Input.Released( InputButton.Jump ) && TimeSinceJumped > GameConfig.SecondsBetweenWormJumps )
				{
					if ( !inputEnabled )
						return;

					TimeSinceJumpReleased = 0;
					HasJumpPending = true;
				}

				// Automatically jump after X seconds.
				if ( TimeSinceJumpReleased > 0.1f && HasJumpPending )
				{
					if ( !inputEnabled )
						return;

					DoJump( ref mover );
					HasJumpPending = false;
				}
			}

			mover.TryMoveWithStep( Time.Delta, Step );
			mover.TryUnstuck();

			// Update our final position and velocity
			Position = mover.Position.WithY( 0 );
			Velocity = mover.Velocity;
		}

		private void DoFriction( ref MoveHelper mover )
		{
			// Drag / friction
			float initialZ = mover.Velocity.z;
			float drag = IsGrounded ? Drag : AirDrag;
			mover.ApplyFriction( drag, Time.Delta );

			// Ignore z friction because it makes no sense
			mover.Velocity.z = initialZ;
		}

		/// <summary>
		/// Set our eye position and rotation
		/// </summary>
		private void SetEyeTransform()
		{
			// Calculate eye position in world.
			EyePosLocal = new Vector3( 0, 0, 24 );

			// Set EyeRot to face the way we're walking.
			LookPos = Velocity.Normal.WithZ( 0 ).IsNearZeroLength ? LookPos : Velocity.WithZ( 0 ).Normal;

			// Only allow aiming changes if the worm isn't moving.
			if ( Velocity.Normal.IsNearlyZero( 2.5f ) && TimeUntilMovementAllowed <= 0 )
			{
				// Aim with W & S keys
				EyeRot = Rotation.LookAt( LookPos );

				LookRotOffset = Math.Clamp( LookRotOffset + Input.Forward * 2, -45, 75 );

				// Rotate EyeRot by our offset
				var targetAxis = LookPos.Normal.x < 0 ? EyeRot.Left : EyeRot.Right;
				EyeRot = EyeRot.RotateAroundAxis( targetAxis, LookRotOffset );
			}

			// Recalculate the worms rotation if we're moving.
			if ( !Velocity.Normal.IsNearZeroLength )
				UpdateWormRotation();
		}

		private void UpdateWormRotation()
		{
			float wormFacing = Pawn.EyeRot.Forward.Dot( Pawn.Rotation.Forward );

			if ( wormFacing < 0 )
			{
				Rotation *= Rotation.From( 0, 180, 0 ); // Super janky
				Pawn.ResetInterpolation();
			}
		}

		/// <summary>
		/// Reproject velocity onto plane
		/// </summary>
		static Vector3 ProjectOntoPlane( Vector3 v, Vector3 normal, float overBounce = 1.0f )
		{
			float backoff = v.Dot( normal );

			if ( overBounce != 1.0 )
			{
				if ( backoff < 0 )
				{
					backoff *= overBounce;
				}
				else
				{
					backoff /= overBounce;
				}
			}

			return v - backoff * normal;
		}

		/// <summary>
		/// Apply an upwards velocity
		/// </summary>
		private void DoJump( ref MoveHelper mover )
		{
			if ( !IsGrounded )
				return;

			mover.Velocity = mover.Velocity.WithZ( Jump );
			GroundEntity = null;

			AddEvent( "jump" );
			TimeSinceJumped = 0;
		}

		private void DoSlide( ref MoveHelper mover, TraceResult trace )
		{
			if ( trace.Normal.Angle( Vector3.Up ) >= mover.MaxStandableAngle && IsGrounded )
			{
				// Don't slide uphill.
				if ( Velocity.Normal.z > 0 )
					return;

				IsSliding = true;
				mover.Velocity = (EyeRot.Forward.Normal + Vector3.Down) * 60;
			}
			else
			{
				IsSliding = false;
			}
		}

		/// <summary>
		/// Check if we're grounded, set ground entity.
		/// </summary>
		private void CheckGroundEntity( ref MoveHelper mover )
		{
			var groundTrace = Trace.Ray( mover.Position, mover.Position + Vector3.Down * 2 ).WorldAndEntities().Ignore( Pawn ).Run();

			if ( groundTrace.Entity is not null )
			{
				if ( GroundEntity is null )
				{
					mover.Velocity = Vector3.Zero;

					TimeUntilMovementAllowed = Math.Abs( FallStartPosZ - mover.Position.z ) * 0.01f;
					FallStartPosZ = -1;
				}

				GroundEntity = groundTrace.Entity;
				Position = Position.WithZ( mover.Position.z.Approach( groundTrace.EndPos.z, Time.Delta ) );

				var worm = Pawn as Worm;
				worm.EquippedWeapon?.ShowWeapon( worm, Velocity.IsNearlyZero( 2.5f ) && IsGrounded );
				worm.IsResolved = !IsSliding;
			}
			else
			{
				GroundEntity = null;
				mover.Velocity += Vector3.Down * 800 * Time.Delta;

				var worm = Pawn as Worm;
				worm.IsResolved = false;
				worm.EquippedWeapon?.ShowWeapon( worm, Velocity.IsNearlyZero( 2.5f ) && IsGrounded );

				if ( FallStartPosZ == -1 )
					FallStartPosZ = mover.Position.z;
			}
		}
	}
}
