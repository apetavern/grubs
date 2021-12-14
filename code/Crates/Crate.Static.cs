using Sandbox;
using TerryForm.Utils;

namespace TerryForm.Crates
{
	public partial class Crate
	{
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

			var crate = Library.Create<Crate>( crateType );

			// TODO: sample from terrain to find a viable spot to plonk a crate down
			crate.Position = new Vector3( Rand.Float( -512, 512 ), 0, 512 );

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
