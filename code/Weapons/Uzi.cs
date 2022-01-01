using Sandbox;
using Grubs.Pawn;

namespace Grubs.Weapons
{
	public class Uzi : Weapon
	{
		public override string WeaponName => "Uzi";
		public override string ModelPath => "models/weapons/uzi/uzi.vmdl";
		public override HoldPose HoldPose => HoldPose.Uzi;
	}
}
