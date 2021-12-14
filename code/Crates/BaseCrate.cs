using Sandbox;
using TerryForm.Pawn;

namespace TerryForm.Crates
{
	/// <summary>
	/// Basic crate logic
	/// </summary>
	[Library( "crate_test" )]
	public class BaseCrate : ModelEntity
	{
		private BBox BBox;

		public override void Spawn()
		{
			base.Spawn();
			SetModel( "models/crates/tools_crate/tools_crate.vmdl" );

			BBox = new( new Vector3( -16, -16, 0 ), new Vector3( 16, 16, 0 ) );
		}

		[Event.Tick.Server]
		public void OnServerTick()
		{
			Move();

			// TODO: Use a fucking trigger
			foreach ( var ent in Physics.GetEntitiesInBox( new BBox( Position + BBox.Mins, Position + BBox.Maxs ) ) )
			{
				if ( ent is Worm )
				{
					this.Delete();
					Log.Trace( $"Worm {ent.Name} picked up crate ({ent.Client.Name})" );
				}
			}
		}

		private void Move()
		{
			var mover = new MoveHelper( Position, Velocity );
			mover.Trace = mover.Trace.Size( BBox ).Ignore( this ).WorldOnly();
			GroundEntity = mover.TraceDirection( Vector3.Down ).Entity;


			const float airResistance = 0.5f;
			if ( GroundEntity == null )
				mover.Velocity += Vector3.Down * PhysicsWorld.Gravity * airResistance * Time.Delta;
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
