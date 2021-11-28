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
		public virtual string ProjectileModel => "";
		public override HoldPose HoldPose => HoldPose.Bazooka;
		public override bool IsFiredTurnEnding => false;

		[Net, Predicted] private float ComputedTraceForce { get; set; }

		private Projectile FiredProjectile { get; set; }

		public override bool CanPrimaryAttack() => WeaponEnabled;

		public override void Simulate( Client player )
		{
			if ( !CanPrimaryAttack() )
				return;

			var windForce = Turn.Instance?.WindForce ?? Vector3.Zero;

			if ( Input.Down( InputButton.Attack1 ) )
			{
				ComputedTraceForce += 0.4f;
				new ArcTrace( Owner.EyePos, (Owner.EyeRot.Forward).Normal, 10 + ComputedTraceForce, windForce ).Run();

				return;
			}

			if ( Input.Released( InputButton.Attack1 ) )
			{
				var trace = new ArcTrace( Owner.EyePos, (Owner.EyeRot.Forward).Normal, 10 + ComputedTraceForce, windForce ).Run();

				if ( IsServer )
					FiredProjectile = new Projectile().WithModel( ProjectileModel ).MoveAlongTrace( trace );
			}

			// Remove this later, it's just so that I can fire a few in a row.
			ComputedTraceForce = 0;

			if ( IsServer )
				FiredProjectile?.Simulate( player );
		}

		public override void OnOwnerKilled() { }

		[ClientRpc]
		public override void OnActiveEffects() { }

		public override void OnFireEffects() { }
	}
}
