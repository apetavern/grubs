using Grubs.Pawn;

namespace Grubs.Weapons
{
	public class Revolver : Weapon
	{
		public override string WeaponName => "Revolver";
		public override string ModelPath => "models/weapons/revolver/revolver.vmdl";
		public override HoldPose HoldPose => HoldPose.Revolver;
	}
}
