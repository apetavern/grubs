using Grubs.Pawn;

namespace Grubs.Weapons
{
	public class Railgun : Weapon
	{
		public override string WeaponName => "Railgun";
		public override string ModelPath => "models/weapons/railgun/railgun.vmdl";
		public override HoldPose HoldPose => HoldPose.Rifle;
	}
}
