using Sandbox;
using Grubs.Pawn;
using Grubs.States.SubStates;
using Grubs.Utils;

namespace Grubs.Weapons
{
	public abstract partial class DroppableWeapon : Weapon
	{
		public override string WeaponName => "";
		public override string ModelPath => "";
		public override int WeaponReach { get; set; } = 100;
		public override bool IsFiredTurnEnding => false;
		public override HoldPose HoldPose => HoldPose.Droppable;

		protected override void Fire()
		{
			// Temp
			var ent = new ModelEntity( ModelPath );
			ent.Position = Parent.Position + Parent.Rotation.Forward * 20f;
			ent.SetupPhysicsFromModel( PhysicsMotionType.Dynamic );

			ent.DeleteAsync( 10 );
		}

		[ClientRpc]
		public override void OnFireEffects()
		{
			// Do particles.
		}
	}
}
