namespace Grubs;

[Prefab]
public partial class FireEmitGadgetComponent : GadgetComponent
{
	[Prefab, Net]
	public float FireSpeed { get; set; } = 850f;

	public override void OnUse( Weapon weapon, int charge )
	{
		FireHelper.StartFiresWithDirection( weapon.GetStartPosition(), Grub.EyeRotation.Forward.Normal * Grub.Facing * FireSpeed, 1 );
		Gadget.Delete();
	}
}
