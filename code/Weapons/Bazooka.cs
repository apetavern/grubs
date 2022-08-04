using Grubs.Player;

namespace Grubs.Weapons;

public class Bazooka : GrubsWeapon
{
	public override string WeaponName => "Bazooka";
	public override string ModelPath => "models/weapons/bazooka/bazooka.vmdl";
	public override HoldPose HoldPose => HoldPose.Bazooka;
}
