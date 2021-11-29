using TerryForm.Pawn;

namespace TerryForm.Weapons
{
	public class Bazooka : ArcWeapon
	{
		public override string WeaponName => "Bazooka";
		public override string ModelPath => "models/weapons/bazooka/bazooka.vmdl";
		public override string ProjectileModel => "models/weapons/shell/shell.vmdl";
		public override HoldPose HoldPose => HoldPose.Bazooka;
		public override bool IsFiredTurnEnding => true;
	}
}
