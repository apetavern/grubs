namespace Grubs;

[Prefab]
public partial class FireGadgetComponent : GadgetComponent
{
	public override void OnUse( Weapon weapon, int charge )
	{
		FireHelper.EmitFiresAt( weapon.GetStartPosition(), Grub.EyeRotation.Forward.Normal * Grub.Facing * 850, 1 );
		Gadget.Delete();
	}
}
