using Sandbox;
using TerryForm.Utils;

namespace TerryForm.Pawn
{
	public class WormController : BasePlayerController
	{
		public float Drag => 8.0f;
		public float AirDrag => 4.0f;
		public float Gravity => 800f;
		public float AirAcceleration => 600f;
		public float Acceleration => 1000f;
		public float Step => 8f;
		public float Jump => 350f;
		public bool IsGrounded => GroundEntity != null;
		private TimeSince TimeSinceJumped { get; set; }
		private Vector3 LookPos { get; set; }

		public override void Simulate()
		{
			var inputEnabled = (Pawn as Worm).IsCurrentTurn;

			SetEyePos( inputEnabled );
			Move( inputEnabled );
		}

		private void Move( bool inputEnabled )
		{
			var mover = new MoveHelper( Position, Velocity );
			mover.Trace = mover.Trace.WorldOnly();
			mover.MaxStandableAngle = 45.0f;

			// Calculate movement speed
			var acceleration = IsGrounded ? Acceleration : AirAcceleration;

			// Calculate/add wish velocity
			Vector3 wishVelocity = inputEnabled ? (-Input.Left * Vector3.Forward) : Vector3.Zero;
			wishVelocity = wishVelocity.Normal * acceleration * Time.Delta;
			mover.Velocity += wishVelocity.WithZ( 0 );

			// Project our velocity onto the current surface we're stood on.
			var groundTrace = mover.TraceDirection( Vector3.Down );
			if ( groundTrace.Hit && groundTrace.Normal.Angle( Vector3.Up ) < mover.MaxStandableAngle )
				mover.Velocity = ProjectOntoPlane( mover.Velocity, groundTrace.Normal );

			if ( Input.Released( InputButton.Jump ) && inputEnabled )
				DoJump( ref mover );

			mover.TryMoveWithStep( Time.Delta, Step );
			mover.TryUnstuck();

			DoFriction( ref mover );

			// Gravity / set our ground entity
			CheckGroundEntity( ref mover );

			// Update our final position and velocity
			Position = mover.Position;
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
		private void SetEyePos( bool inputEnabled )
		{
			if ( !inputEnabled )
				return;

			// Calculate eye position in world.
			EyePosLocal = new Vector3( 0, 0, 24 );
			var eyePos = Pawn.Transform.PointToWorld( EyePosLocal );

			// Set EyeRot to face the way we're walking.
			LookPos = Velocity.Normal.IsNearZeroLength ? LookPos : Velocity.Normal.WithZ( LookPos.z );

			// Aim with W & S keys
			LookPos += Input.Forward * Vector3.Up * 0.025f;

			EyeRot = Rotation.LookAt( LookPos );

			// Recalculate the worms rotation if we're moving.
			if ( !Velocity.IsNearZeroLength )
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
			if ( !IsGrounded || TimeSinceJumped < GameConfig.SecondsBetweenWormJumps )
				return;

			mover.Velocity = mover.Velocity.WithZ( Jump );
			GroundEntity = null;

			AddEvent( "jump" );
			TimeSinceJumped = 0;
		}

		/// <summary>
		/// Check if we're grounded, set ground entity.
		/// </summary>
		private void CheckGroundEntity( ref MoveHelper mover )
		{
			var groundTrace = Trace.Ray( mover.Position, mover.Position + Vector3.Down ).WorldOnly().Run();

			if ( groundTrace.Entity is not null )
			{
				GroundEntity = groundTrace.Entity;
				Position = Position.WithZ( mover.Position.z.Approach( groundTrace.EndPos.z, Time.Delta ) );

				var worm = Pawn as Worm;
				worm.EquippedWeapon?.ShowWeapon( worm, Velocity.IsNearlyZero( 2.5f ) && IsGrounded );
				worm.IsResolved = true;
			}
			else
			{
				GroundEntity = null;
				mover.Velocity += Vector3.Down * 800 * Time.Delta;
			}
		}
	}
}
