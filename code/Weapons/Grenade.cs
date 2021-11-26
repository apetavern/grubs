using TerryForm.Pawn;

namespace TerryForm.Weapons
{
	public class Grenade : ArcWeapon
	{
		public override string WeaponName => "Grenade";
		public override string ModelPath => "models/weapons/grenade/grenade.vmdl";
		public override HoldPose HoldPose => HoldPose.Throwable;
		public override float PrimaryRate => 2f;

		public override void Fire()
		{


			base.Fire();
		}
	}
}
