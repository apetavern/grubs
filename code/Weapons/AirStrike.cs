using Sandbox;
using Grubs.Pawn;

namespace Grubs.Weapons
{
	public class AirStrike : AirWeapon
	{
		public override string WeaponName => "Air Strike";
		public override string ModelPath => "models/weapons/airstrikes/radio.vmdl";
		public override HoldPose HoldPose => HoldPose.Holdable;
	}
}
