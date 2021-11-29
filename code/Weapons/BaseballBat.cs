using TerryForm.Pawn;

namespace TerryForm.Weapons
{
	public class BaseballBat : Weapon
	{
		public override string WeaponName => "Baseball Bat";
		public override string ModelPath => "models/weapons/baseballbat/baseballbat.vmdl";
		public override HoldPose HoldPose => HoldPose.Swing;
		public override float PrimaryRate => 2f;
		public override bool IsFiredTurnEnding => true;

		public override void Fire()
		{
			base.Fire();
		}
	}
}
