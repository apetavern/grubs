using Sandbox;
using TerryForm.Pawn;

namespace TerryForm.Crates
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
