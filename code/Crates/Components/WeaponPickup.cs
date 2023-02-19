namespace Grubs;

[Prefab]
public partial class WeaponPickup : PickupComponent
{
	public override void OnPickup( Grub grub )
	{
		var weapon = CrateDropTables.GetRandomWeaponFromCrate();
	}
}
