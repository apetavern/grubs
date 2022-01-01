using Sandbox;
using Grubs.Pawn;

namespace Grubs.Crates
{
	[Library( "crate_weapons" )]
	public class WeaponsCrate : Crate
	{
		public override void Spawn()
		{
			base.Spawn();
			SetModel( "models/crates/weapons_crate/weapons_crate.vmdl" );
		}

		protected override void OnPickup( Worm worm )
		{
			base.OnPickup( worm );

			// TODO
		}
	}
}
