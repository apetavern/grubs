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
		public float Step => 16f;
		public float Jump => 650f;
		public bool IsGrounded => GroundEntity != null;

		private RealTimeUntil timeUntilCanMove = -1f;
		private float jumpStartZ;

		// Cooldown multiplier for jumping; how many seconds should we wait for each unit we fell
		public float MovementCooldownMultiplier => 0.01f;

		public override void Simulate()
		{
			BBox = CalcBbox();

			SetEyePos();
			Move();
		}

		private BBox BBox { get; set; }
		public BBox CalcBbox()
		{
			var bbox = new BBox( new Vector3( -16, -16, 0 ), new Vector3( 16, 16, 32 ) );
			return bbox;
		}

		/// <summary>
		/// Set our eye position & rotation
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
			DebugOverlay.Line( eyePos, eyePos + EyeRot.Forward * 128, 0, false );
		}

		private void Move()
		{
			MoveHelper mover = new( Position, Velocity );
			mover.Trace = mover.Trace.Size( BBox ).Ignore( Pawn );

			CheckGroundEntity( ref mover ); // Gravity start

			// Accelerate in whatever direction the player is pressing...
			Vector3 wishVelocity = -Input.Left * Vector3.Forward;
			// ...but not upwards
			wishVelocity.z = 0;

			//
			// Acceleration
			//
			if ( timeUntilCanMove <= 0 )
			{
				float accel = IsGrounded ? Acceleration : AirAcceleration;
				wishVelocity = wishVelocity.Normal * accel * Time.Delta;
				mover.Velocity += wishVelocity;
			}

			CheckGroundEntity( ref mover ); // Gravity end


			//
			// Jumping
			//
			if ( timeUntilCanMove <= 0 )
			{
				if ( Input.Pressed( InputButton.Jump ) && IsGrounded )
				{
					DoJump( ref mover );
					AddEvent( "jump" );
				}
			}

			//
			// Drag / friction
			//
			float initialZ = mover.Velocity.z;
			float drag = IsGrounded ? Drag : AirDrag;
			mover.ApplyFriction( drag, Time.Delta );
			// Ignore z friction because it makes no sense
			mover.Velocity.z = initialZ;

			mover.TryMoveWithStep( Time.Delta, Step );
			mover.TryUnstuck();
			Position = mover.Position;
			Velocity = mover.Velocity;

			if ( IsGrounded )
				StayOnGround( mover );
		}

		/// <summary>
		/// Try to stick to the ground if we can
		/// </summary>
		private void StayOnGround( MoveHelper mover )
		{
			var start = Position + Vector3.Up * 2;
			var end = Position + Vector3.Down * Step;

			// See how far up we can go without getting stuck
			var trace = mover.TraceFromTo( Position, start );
			start = trace.EndPos;

			// Now trace down from a known safe position
			trace = mover.TraceFromTo( start, end );

			if ( trace.Fraction <= 0 ) return;
			if ( trace.Fraction >= 1 ) return;
			if ( trace.StartedSolid ) return;
			if ( Vector3.GetAngle( Vector3.Up, trace.Normal ) > 45f ) return;

			Position = trace.EndPos;
		}

		/// <summary>
		/// Apply an upwards velocity
		/// </summary>
		private void DoJump( ref MoveHelper mover )
		{
			mover.Velocity = mover.Velocity.WithZ( Jump );
			GroundEntity = null;
			Pawn.GroundEntity = null;

			jumpStartZ = mover.Position.z;
		}

		/// <summary>
		/// Check if we're grounded
		/// </summary>
		private void CheckGroundEntity( ref MoveHelper mover )
		{
			var targetPos = Position + Vector3.Down;
			var tr = Trace.Ray( Position, targetPos ).WorldOnly().Size( BBox ).Run();

			if ( tr.Hit )
			{
				// Have we just landed?
				if ( GroundEntity == null )
				{
					float jumpDelta = jumpStartZ - mover.Position.z;
					timeUntilCanMove = jumpDelta * MovementCooldownMultiplier;
				}

				GroundEntity = tr.Entity;
			}
			else
			{
				GroundEntity = null;
				mover.Velocity += Vector3.Down * Gravity * Time.Delta;
			}

			// Hide the worms weapon if we aren't grounded.
			(Pawn as Worm).EquippedWeapon?.SetVisible( tr.Hit );
		}
	}
}
