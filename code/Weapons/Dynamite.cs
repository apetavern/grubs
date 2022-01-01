using Grubs.Pawn;

namespace Grubs.Weapons
{
	// ArcWeapon doesn't seem to fit the purpose of weapons with projectiles that aren't thrown.
	// This should be changed to something like PlacedWeapon.
	public class Dynamite : ArcWeapon
	{
		public override string WeaponName => "Dynamite";
		public override string ModelPath => "models/weapons/dynamite/dynamite.vmdl";
		public override string ProjectileModel => ModelPath;
		public override HoldPose HoldPose => HoldPose.Throwable;
	}
}
