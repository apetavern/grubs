using Sandbox;
using Grubs.Pawn;
using Grubs.Utils;
using Grubs.Weapons;

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

			CrateDropTables.WeaponDropTypes weapon = CrateDropTables.GetRandomWeaponFromCrate();

			var player = (worm.Owner as Pawn.Player);
			player.PlayerInventory.Add( Library.Create<Weapon>( weapon.ToString() ) );
		}
	}
}
