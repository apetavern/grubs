namespace Grubs;

public class WeaponComponent : EntityComponent<Weapon>
{
	protected Weapon Weapon => Entity;
	protected Grub Grub => Weapon.Owner as Grub;
	protected Player Player => Grub.Player;
}
