using Sandbox;

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
		public float Jump => 650f;
		public bool IsGrounded => GroundEntity != null;

		public override void Simulate()
		{
			var inputEnabled = (Pawn as Worm).IsCurrentTurn;

			Move( inputEnabled );

			if ( inputEnabled )
				SetEyePos();
		}

		private void Move( bool inputAllowed )
		{
			var mover = new MoveHelper( Position, Velocity );
			mover.Trace = mover.Trace.WorldOnly();
			mover.MaxStandableAngle = 45.0f;

			// Calculate movement speed
			var acceleration = IsGrounded ? Acceleration : AirAcceleration;

			// Calculate/add wish velocity
			Vector3 wishVelocity = (-Input.Left * Vector3.Forward);
			wishVelocity = wishVelocity.Normal * acceleration * Time.Delta;
			mover.Velocity += wishVelocity.WithZ( 0 );

			// Project our velocity onto the current surface we're stood on.
			var groundTrace = mover.TraceDirection( Vector3.Down );
			if ( groundTrace.Hit && groundTrace.Normal.Angle( Vector3.Up ) < mover.MaxStandableAngle )
				mover.Velocity = ProjectOntoPlane( mover.Velocity, groundTrace.Normal );

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
		private void SetEyePos()
		{
			EyePosLocal = new Vector3( 0, 0, 24 );
			var eyePos = Pawn.Transform.PointToWorld( EyePosLocal );

			var plane = new Plane( Position, Vector3.Right );
			var projectedCursorPosition = plane.Trace( new Ray( Input.Cursor.Origin, Input.Cursor.Direction ) ) ?? default;

			var eyeDirection = projectedCursorPosition - eyePos;
			eyeDirection = eyeDirection.Normal;

			EyeRot = Rotation.LookAt( eyeDirection );

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

		}

		/// <summary>
		/// Check if we're grounded, set ground entity.
		/// </summary>
		private void CheckGroundEntity( ref MoveHelper mover )
		{
			var groundTrace = Trace.Ray( mover.Position, mover.Position + Vector3.Down * 2 ).WorldOnly().Run();

			DebugOverlay.Line( groundTrace.StartPos, groundTrace.EndPos );

			if ( groundTrace.Entity is not null )
			{
				GroundEntity = groundTrace.Entity;
				Position = Position.WithZ( mover.Position.z.Approach( groundTrace.EndPos.z, Time.Delta ) );
			}
			else
			{
				GroundEntity = null;
				mover.Velocity += Vector3.Down * 800 * Time.Delta;
			}
		}
	}
}
