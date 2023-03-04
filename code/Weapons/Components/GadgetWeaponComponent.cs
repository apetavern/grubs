namespace Grubs;

[Prefab]
public partial class GadgetWeaponComponent : WeaponComponent
{
	[Prefab, Net]
	public string GadgetPrefabPath { get; set; }

	public override void Simulate( IClient client )
	{
		base.Simulate( client );

		if ( IsFiring )
			Fire();
	}

	public override void FireCursor()
	{
		FireCharged();
	}

	public override void FireInstant()
	{
		FireCharged();
	}

	public override void FireCharged()
	{
		if ( !Game.IsServer )
			return;

		if ( PrefabLibrary.TrySpawn<Gadget>( GadgetPrefabPath, out var gadget ) )
			gadget.OnUse( Grub, Weapon, Charge );

		Grub.SetAnimParameter( "fire", true );

		IsFiring = false;
		Charge = MinCharge;

		FireFinished();
	}
}
