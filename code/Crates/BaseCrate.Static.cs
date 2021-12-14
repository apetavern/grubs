using Sandbox;
using TerryForm.Utils;

namespace TerryForm.Crates
{
	public partial class Crate
	{
		public static Crate SpawnCrate()
		{
			Host.AssertServer();

			var crateType = Rand.FromArray( GameConfig.CrateTypes );
			var crate = Library.Create<Crate>( crateType );

			return crate;
		}

		[ServerCmd]
		public static void ForceSpawnCrate()
		{
			var crate = SpawnCrate();
			crate.Position = new Vector3( 0, 0, 128 );
			Log.Trace( $"Spawned crate" );
		}
	}
}
