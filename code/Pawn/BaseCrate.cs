using Sandbox;

namespace TerryForm.Pawn
{
	/// <summary>
	/// Basic crate logic
	/// </summary>
	[Library( "crate_test" )]
	public class BaseCrate : ModelEntity
	{
		public override void Spawn()
		{
			base.Spawn();
			SetModel( "models/crates/tools_crate/tools_crate.vmdl" );
			SetupPhysicsFromModel( PhysicsMotionType.Keyframed, false );
		}

		[Event.Tick.Server]
		public void OnServerTick()
		{
			/*
			 * "why are we using movehelpers here?"
			 * well i'm glad you asked
			 * 
			 * - we don't want explosions to hit these and have them fly off in some random direction
			 * - we want more control over gravity
			 * - we want them to be axis aligned - cos we're in 2D
			 */

			var mover = new MoveHelper( Position, Velocity );
			mover.Trace = mover.Trace.Size( new Vector3( -16, -16, 0 ), new Vector3( 16, 16, 0 ) ).Ignore( this ).WorldOnly();
			GroundEntity = mover.TraceDirection( Vector3.Down ).Entity;

			if ( GroundEntity == null )
				mover.Velocity += Vector3.Down * 400 * Time.Delta;
			else
				mover.Velocity = 0;

			mover.TryMove( Time.Delta );

			this.Position = mover.Position;
			this.Velocity = mover.Velocity;
		}

		[ServerCmd]
		public static void SpawnTestCrate()
		{
			_ = new BaseCrate()
			{
				Position = new Vector3( 0, 0, 128 )
			};
			Log.Trace( $"Spawned basic test crate" );
		}
	}
}
