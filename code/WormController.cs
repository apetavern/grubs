using Sandbox;

namespace TerryForm
{
	public class WormController : BasePlayerController
	{
		public float Drag => 2.0f;
		public float AirDrag => 1.5f;
		public float Gravity => 800;
		public float Acceleration => 1200;
		public float Step => 16;
		public bool IsGrounded => GroundEntity != null;

		public override void Simulate()
		{
			BBox = CalcBbox();

			SetEyePos();
			Move();

			DebugOverlay.Line( Position, Position + Rotation.Forward * 32, 0, false );
			DebugOverlay.Sphere( Position + EyePosLocal, 0.5f, Color.Red, false, 0 );
		}

		private BBox BBox { get; set; }
		public BBox CalcBbox()
		{
			var bbox = new BBox( new Vector3( -16, -16, 0 ), new Vector3( 16, 16, 72 ) );
			return bbox;
		}

		/// <summary>
		/// Set our eye position & rotation
		/// </summary>
		private void SetEyePos()
		{
			// EyeRot = Input.Rotation; // TODO
			EyePosLocal = new Vector3( 0, 0, 64 );
		}

		private void Move()
		{
			MoveHelper mover = new( Position, Velocity );
			mover.Trace = mover.Trace.Size( BBox ).Ignore( Pawn );

			CheckGroundEntity( ref mover ); // Gravity start

			Vector3 vel = -Input.Left * Rotation.Forward;
			vel.z = 0;

			vel = vel.Normal * Acceleration * Time.Delta;
			mover.Velocity += vel;

			if ( Input.Down( InputButton.Jump ) && IsGrounded )
				DoJump( ref mover );

			CheckGroundEntity( ref mover ); // Gravity end

			mover.TryMoveWithStep( Time.Delta, Step );
			mover.TryUnstuck();
			mover.ApplyFriction( IsGrounded ? Drag : AirDrag, Time.Delta );

			Position = mover.Position;
			Velocity = mover.Velocity;
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
			mover.Velocity += Vector3.Up * 512;
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
				GroundEntity = tr.Entity;
			}
			else
			{
				GroundEntity = null;
				mover.Velocity += Vector3.Down * Gravity * Time.Delta;
			}
		}
	}
}
