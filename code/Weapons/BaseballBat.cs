using Grubs.Pawn;

namespace Grubs.Weapons
{
	public class BaseballBat : Weapon
	{
		public override string WeaponName => "Baseball Bat";
		public override string ModelPath => "models/weapons/baseballbat/baseballbat.vmdl";
		public override HoldPose HoldPose => HoldPose.Swing;

		protected override void Fire()
		{
			base.Fire();
		}
	}
}
