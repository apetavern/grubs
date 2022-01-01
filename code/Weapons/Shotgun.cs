using Sandbox;
using Grubs.Pawn;

namespace Grubs.Weapons
{
	public class Shotgun : Weapon
	{
		public override string WeaponName => "Shotgun";
		public override string ModelPath => "models/weapons/shotgun/shotgun.vmdl";
		public override HoldPose HoldPose => HoldPose.Shotgun;
	}
}
