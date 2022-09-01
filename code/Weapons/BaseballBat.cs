using Grubs.Weapons.Base;

namespace Grubs.Weapons;

/// <summary>
/// A simple baseball bat.
/// </summary>
public class BaseballBat : MeleeWeapon
{
	protected override string WeaponName => "Baseball Bat";
	protected override string ModelPath => "models/weapons/baseballbat/baseballbat.vmdl";
	protected override HoldPose HoldPose => HoldPose.Swing;

	protected override Vector3 HitSize => new( 50, 32, 50 );
	protected override float Damage => 25;
}
