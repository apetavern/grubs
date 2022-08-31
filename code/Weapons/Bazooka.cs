using Grubs.Weapons.Base;

namespace Grubs.Weapons;

/// <summary>
/// The classic Bazooka.
/// </summary>
public class Bazooka : ProjectileWeapon
{
	protected override string WeaponName => "Bazooka";
	protected override string ModelPath => "models/weapons/bazooka/bazooka.vmdl";
	protected override FiringType FiringType => FiringType.Charged;
	protected override HoldPose HoldPose => HoldPose.Bazooka;

	protected override float ProjectileForceMultiplier => 0.5f;
	protected override string ProjectileModel => "models/weapons/shell/shell.vmdl";
	protected override float ProjectileSpeed => 1000;
	protected override float ProjectileExplosionRadius => 100;
	protected override float ProjectileExplodeAfter => 5;
}
