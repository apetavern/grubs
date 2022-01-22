using Grubs.Pawn;

namespace Grubs.Weapons
{
	public class Revolver : Weapon
	{
		public override string WeaponName => "Revolver";
		public override string ModelPath => "models/weapons/sixshooter/sixshooter.vmdl";
		public override HoldPose HoldPose => HoldPose.Revolver;
	}
}
