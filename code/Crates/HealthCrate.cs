using Sandbox;
using Grubs.Pawn;

namespace Grubs.Crates
{
	[Library( "crate_health" )]
	public class HealthCrate : Crate
	{
		public override void Spawn()
		{
			base.Spawn();
			SetModel( "models/crates/health_crate/health_crate.vmdl" );
		}

		protected override void OnPickup( Worm worm )
		{
			base.OnPickup( worm );

			worm.GiveHealth( 50 );
		}
	}
}
