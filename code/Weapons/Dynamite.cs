using Grubs.Pawn;

namespace Grubs.Weapons
{
	public class Dynamite : PlacedWeapon
	{
		public override string WeaponName => "Dynamite";
		public override string ModelPath => "models/weapons/dynamite/dynamite.vmdl";
		public override HoldPose HoldPose => HoldPose.Droppable;
	}
}
