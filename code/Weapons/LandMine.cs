using Grubs.Pawn;

namespace Grubs.Weapons
{
	public class LandMine : PlacedWeapon
	{
		public override string WeaponName => "Land Mine";
		public override string ModelPath => "models/weapons/landmine/landmine.vmdl";
		public override HoldPose HoldPose => HoldPose.Droppable;
	}
}
