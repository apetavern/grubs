using Grubs.Weapons.Base;

namespace Grubs.Weapons;

/// <summary>
/// A throwable grenade.
/// </summary>
public class Grenade : ProjectileWeapon
{
	protected override string WeaponName => "Grenade";
	protected override string ModelPath => "models/weapons/grenade/grenade.vmdl";
	protected override FiringType FiringType => FiringType.Charged;
	protected override HoldPose HoldPose => HoldPose.Throwable;
	public override bool HasReticle => true;

	protected override float ProjectileForceMultiplier => 0.4f;
	protected override bool ProjectileShouldBounce => true;
	protected override int ProjectileMaxBounces => 5;
	protected override string ProjectileModel => "models/weapons/grenade/grenade.vmdl";
	protected override float ProjectileSpeed => 1000;
	protected override float ProjectileExplosionRadius => 100;
	protected override float ProjectileCollisionExplosionDelay => 3;
}
