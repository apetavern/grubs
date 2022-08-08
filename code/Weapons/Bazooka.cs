using Grubs.Weapons.Projectiles;

namespace Grubs.Weapons;

public class Bazooka : GrubsWeapon
{
	public override string WeaponName => "Bazooka";
	public override string ModelPath => "models/weapons/bazooka/bazooka.vmdl";
	public override string ProjectileModelPath => "models/weapons/shell/shell.vmdl";
	public override FiringType FiringType => FiringType.Charged;
	public override HoldPose HoldPose => HoldPose.Bazooka;
}
