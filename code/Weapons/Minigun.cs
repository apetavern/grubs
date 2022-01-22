using Sandbox;
using Grubs.Pawn;

namespace Grubs.Weapons
{
	public class Minigun : Weapon
	{
		public override string WeaponName => "Minigun";
		public override string ModelPath => "models/weapons/minigun/minigun.vmdl";
		public override HoldPose HoldPose => HoldPose.Minigun;
	}
}
