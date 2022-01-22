using Grubs.Pawn;

namespace Grubs.Weapons
{
	public class PetrolBomb : ArcWeapon
	{
		public override string WeaponName => "Petrol Bomb";
		public override string ModelPath => "models/weapons/petrolbomb/petrolbomb.vmdl";
		public override string ProjectileModel => ModelPath;
		public override HoldPose HoldPose => HoldPose.Throwable;
	}
}
