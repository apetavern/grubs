using Sandbox;
using Grubs.Utils;
using Grubs.Weapons.Helpers;

namespace Grubs.Crates
{
	public partial class Crate
	{
		public static int ActiveCrateCount { get; set; }

		private static string GetRandomCrate()
		{
			float val = Rand.Float();
			float cumulativeValue = 0;
			foreach ( var crateType in GameConfig.CrateTypes )
			{
				cumulativeValue += crateType.Value;

				if ( val < cumulativeValue )
				{
					return crateType.Key;
				}
			}

			return null;
		}

		public static Crate SpawnCrate()
		{
			Host.AssertServer();

			var crateType = GetRandomCrate();

			if ( string.IsNullOrEmpty( crateType ) )
			{
				Log.Trace( "Missed random chance" );
				return null;
			}

			if ( ActiveCrateCount >= GameConfig.MaxActiveCrates )
			{
				Log.Trace( "Skipped spawning crate, too many are active." );
				return null;
			}

			var crate = Library.Create<Crate>( crateType );
			AirDropHelper.SummonDropWithTarget( crate, new Vector3( Rand.Float( -1000, 1000 ), 0, 0 ) );

			ActiveCrateCount++;

			return crate;
		}

		[ServerCmd]
		public static void ForceSpawnCrate()
		{
			Rand.SetSeed( Time.Tick );
			var crate = SpawnCrate();

			if ( crate.IsValid() )
			{
				Log.Trace( $"Spawned crate" );
			}
		}
	}
}
