using Sandbox;
using TerryForm.Pawn;

namespace TerryForm.Crates
{
	/// <summary>
	/// Basic crate logic
	/// </summary>
	public partial class Crate : ModelEntity
	{
		private BBox BBox;

		public override void Spawn()
		{
			base.Spawn();

			// Show a big fat question mark if someone forgot to set crate model
			SetModel( "models/editor/proxy_helper.vmdl" );
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
				mover.Velocity += (PhysicsWorld.Gravity * airResistance) * Time.Delta;
			else
				mover.Velocity = 0;

			mover.TryMove( Time.Delta );

			this.Position = mover.Position;
			this.Velocity = mover.Velocity;
		}
	}
}
