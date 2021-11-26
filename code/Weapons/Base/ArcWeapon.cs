using Sandbox;
using TerryForm.Pawn;
using TerryForm.States.SubStates;
using TerryForm.Utils;

namespace TerryForm.Weapons
{
	public abstract partial class ArcWeapon : Weapon
	{
		public override string WeaponName => "";
		public override string ModelPath => "";
		public override float PrimaryRate => 2f;
		public override HoldPose HoldPose => HoldPose.Bazooka;
		public override bool IsFiredTurnEnding => false;

		private float InitialTraceForce { get; set; } = 20;
		private float ComputedTraceForce { get; set; }
		private Projectile FiredProjectile { get; set; }

		public override bool CanPrimaryAttack()
		{
			if ( !WeaponEnabled )
				return false;

			return base.CanPrimaryAttack();
		}

		public async override void Fire()
		{
			base.Fire();


		}

		public override void Simulate( Client player )
		{
			if ( Input.Down( InputButton.Attack1 ) )
			{
				ComputedTraceForce += 0.4f;
				new ArcTrace( Owner.EyePos, (Owner.EyeRot.Forward).Normal, InitialTraceForce + ComputedTraceForce ).Run();
			}

			if ( Input.Released( InputButton.Attack1 ) )
			{
				var trace = new ArcTrace( Owner.EyePos, (Owner.EyeRot.Forward).Normal, InitialTraceForce + ComputedTraceForce ).Run();
				ComputedTraceForce = 0;

				FiredProjectile = new Projectile().MoveAlongTrace( trace );
			}

			FiredProjectile?.Simulate( player );
		}

		public override void OnOwnerKilled() { }

		[ClientRpc]
		public override void OnActiveEffects() { }

		public override void OnFireEffects() { }
	}
}
