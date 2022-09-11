using Grubs.Player;
using Grubs.Utils;

namespace Grubs.Crates;

/// <summary>
/// A crate capable of carrying weapons for Grubs to use.
/// </summary>
public class WeaponCrate : BaseCrate
{
	public override void Spawn()
	{
		base.Spawn();

		SetModel( "models/crates/weapons_crate/weapons_crate.vmdl" );
	}

	public override void OnPickup( Grub grub )
	{
		base.OnPickup( grub );

		var weapon = CrateDropTables.GetRandomWeaponFromCrate();
		grub.Team.GiveAmmo( weapon, 1 );
	}
}
