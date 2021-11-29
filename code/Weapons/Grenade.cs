using TerryForm.Pawn;

namespace TerryForm.Weapons
{
	public class Grenade : ArcWeapon
	{
		public override string WeaponName => "Grenade";
		public override string ModelPath => "models/weapons/grenade/grenade.vmdl";
		public override string ProjectileModel => ModelPath;
		public override HoldPose HoldPose => HoldPose.Throwable;
	}
}
