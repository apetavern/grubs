using TerryForm.Pawn;

namespace TerryForm.Weapons
{
	public class BaseballBat : Weapon
	{
		public override string WeaponName => "Baseball Bat";
		public override string ModelPath => "models/weapons/baseballbat/baseballbat.vmdl";
		public override HoldPose HoldPose => HoldPose.Swing;
		public override bool IsFiredTurnEnding => true;

		protected override void Fire()
		{
			base.Fire();
		}
	}
}
