using TerryForm.Pawn;

namespace TerryForm.Weapons
{
	public class Bazooka : ArcWeapon
	{
		public override string WeaponName => "Bazooka";
		public override string ModelPath => "models/weapons/bazooka/bazooka.vmdl";
		public override HoldPose HoldPose => HoldPose.Bazooka;
		public override float PrimaryRate => 2f;

		public override void Fire()
		{


			base.Fire();
		}
	}
}
