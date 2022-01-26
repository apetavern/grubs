using System;
using Sandbox;
using System.Collections.Generic;
using System.Linq;

namespace Grubs.Weapons.Helpers
{
	public partial class FireHelper : Entity
	{
		static FireHelper Instance { get; set; }
		public Dictionary<Entity, TimeSince> FireInstances { get; set; }
		private int FireTickRate { get; set; } = 2;

		public static void StartFireAt( Vector3 origin, int qty )
		{
			Host.AssertServer();

			if ( !Host.IsServer )
				return;

			if ( Instance is null || !Instance.IsValid )
				Instance = new();

			// Create instances of fire, delay their initial ticks.
			for ( int i = 0; i < qty; i++ )
				Instance.FireInstances.Add( new Entity(), Rand.Float( 0.3f ) );
		}

		[Event.Tick.Server]
		public void TickFire()
		{
			if ( !FireInstances.Any() )
				return;

			foreach ( var fire in FireInstances )
			{
				var fireInstance = fire.Key;
				var timeSinceLastTick = fire.Value;

				// Move the fire
				// movement stuff

				if ( timeSinceLastTick > FireTickRate )
				{
					// Damage terrain

					// Reset the last tick timer.
					FireInstances[fireInstance] = 0;
				}
			}
		}

		[ClientRpc]
		public static void DoFireEffectsAt( Vector3 position )
		{
			// somewhere
		}
	}
}
