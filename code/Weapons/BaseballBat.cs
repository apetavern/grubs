namespace Grubs.Weapons;

public class BaseballBat : GrubWeapon
{
	public override string WeaponName => "Baseball Bat";
	public override string ModelPath => "models/weapons/baseballbat/baseballbat.vmdl";
	public override HoldPose HoldPose => HoldPose.Swing;
}
