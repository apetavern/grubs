using Sandbox;
using System.Collections.Generic;
using System.Linq;
using Grubs.Terrain;
using Grubs.Crates;

namespace Grubs.Weapons.Helpers
{
	public partial class Fire : Entity
	{
		private float FireTickRate { get; set; } = 0.5f;
		private TimeSince TimeSinceLastTick { get; set; }
		private float FireLifeDuration { get; set; } = 5;
		private float ExpiryTime { get; set; }
		public bool IsResolved { get; set; }
		public Vector3 MoveDirection { get; set; }

		public Fire( Vector3 startPos, Vector3 movementDirection )
		{
			ExpiryTime = Time.Now + 5;
			TimeSinceLastTick = Rand.Float( 0.5f );
			Position = startPos + new Vector3().WithX( Rand.Int( 30 ) );
			MoveDirection = movementDirection;
		}

		private void Move()
		{
			SDF circle = new Circle( Position, 10f, SDF.MergeType.Subtract );
			Terrain.Terrain.Update( circle );
		}

		public void Tick()
		{
			if ( Time.Now > ExpiryTime )
				Delete();

			Move();
		}
	}

	public partial class FireHelper : Entity
	{
		static FireHelper Instance { get; set; }
		public List<Fire> FireInstances { get; set; } = new();

		public static void StartFiresAt( Vector3 origin, Vector3 moveDirection, int qty )
		{
			Host.AssertServer();

			if ( !Host.IsServer )
				return;

			if ( Instance is null || !Instance.IsValid )
				Instance = new();

			Instance.Position = origin;

			// Create instances of fire, delay their initial ticks and set their death time.
			for ( int i = 0; i < qty; i++ )
			{
				var fire = new Fire( Instance.Position + Vector3.Random.WithY( 0 ) * 20, moveDirection );

				Instance.FireInstances.Add( fire );
			}
		}

		[Event.Tick.Server]
		public void TickFire()
		{
			if ( !FireInstances.Any() )
				return;

			foreach ( var fire in FireInstances )
			{
				if ( fire.IsValid() )
					fire.Tick();
			}
		}

		[ClientRpc]
		public static void DoFireEffectsAt( Vector3 position )
		{
			// somewhere
		}
	}
}
