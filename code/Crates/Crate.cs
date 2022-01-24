using Sandbox;
using Grubs.Pawn;
using Grubs.Weapons;
using System.Linq;
using Grubs.Weapons.Helpers;

namespace Grubs.Crates
{
	/// <summary>
	/// Basic crate logic
	/// </summary>
	public partial class Crate : ModelEntity
	{
		private BBox BBox;

		private CrateTrigger trigger;

		public override void Spawn()
		{
			base.Spawn();

			// Show a big fat question mark if someone forgot to set crate model
			SetModel( "models/editor/proxy_helper.vmdl" );

			BBox = new( new Vector3( -16, -16, 0 ), new Vector3( 16, 16, 16 ) );

			Health = 25;

			trigger = new CrateTrigger();
			trigger.Position = Position;
			trigger.SetParent( this );
		}

		public override void Touch( Entity other )
		{
			base.Touch( other );

			Log.Trace( $"Touched {other}" );
			if ( other is Worm worm )
			{
				ActiveCrateCount--;
				OnPickup( worm );

				Delete();
			}
		}

		[Event.Tick.Server]
		public void OnServerTick()
		{
			Move();

			if ( Health <= 0 )
			{
				ExplosionHelper.DoBlastWithRadius( Position, 75 );

				ActiveCrateCount--;
				Delete();
			}
		}

		private void Move()
		{
			var mover = new MoveHelper( Position, Velocity );
			mover.Trace = mover.Trace.Size( BBox ).Ignore( this ).WorldAndEntities();
			GroundEntity = mover.TraceDirection( Vector3.Down ).Entity;

			if ( GroundEntity == null )
				mover.Velocity += PhysicsWorld.Gravity * Time.Delta;
			else
				mover.Velocity = 0;

			const float airResistance = 2.0f;
			mover.ApplyFriction( airResistance, Time.Delta );
			mover.TryMove( Time.Delta );

			this.Position = mover.Position;
			this.Velocity = mover.Velocity;
		}

		protected virtual void OnPickup( Worm worm )
		{
			Log.Trace( $"Worm {worm.Name} picked up crate ({worm.Client.Name})" );
		}

		[ServerCmd]
		public static void SetCrateHealthToZero()
		{
			foreach ( var crate in Entity.All.OfType<Crate>() )
			{
				crate.Health = 0;
			}
		}
	}
}
