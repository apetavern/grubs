namespace Grubs;

[Prefab]
public partial class DroppableExplosiveComponent : ExplosiveComponent
{
	public override void OnFired( Weapon weapon, int charge )
	{
		base.OnFired( weapon, charge );

		Explosive.Position = (Grub.EyePosition + (Grub.Facing * 20f)).WithY( 0 );
	}
}
