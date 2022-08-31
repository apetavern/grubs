using Grubs.Weapons.Base;

namespace Grubs.Weapons;

/// <summary>
/// A simple baseball bat.
/// </summary>
public class BaseballBat : GrubWeapon
{
	protected override string WeaponName => "Baseball Bat";
	protected override string ModelPath => "models/weapons/baseballbat/baseballbat.vmdl";
	protected override HoldPose HoldPose => HoldPose.Swing;
}
