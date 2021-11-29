using Sandbox;
using TerryForm.Pawn;
using TerryForm.States.SubStates;

namespace TerryForm.Weapons
{
	public abstract partial class ArcWeapon : Weapon
	{
		public override string WeaponName => "";
		public override string ModelPath => "";
		public virtual string ProjectileModel => "";
		public override HoldPose HoldPose => HoldPose.Bazooka;
	}
}
