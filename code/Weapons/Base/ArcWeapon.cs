using Sandbox;
using TerryForm.Pawn;
using TerryForm.States.SubStates;

namespace TerryForm.Weapons
{
	public abstract partial class ArcWeapon : Weapon
	{
		// Weapon settings
		public override string WeaponName => "";
		public override string ModelPath => "";
		public virtual string ProjectileModel => "";
		public override HoldPose HoldPose => HoldPose.Bazooka;

		// Weapon properties
		public Entity Projectile { get; set; }
		private float ComputedForce { get; set; } = 10;

		public override void Simulate( Client player )
		{
			if ( Input.Down( InputButton.Attack1 ) && WeaponEnabled && TimeSinceFired > SecondsBetweenFired )
			{
				ComputedForce += 0.4f;
				Fire();

				return;
			}

			if ( Input.Released( InputButton.Attack1 ) )
			{
				QuantityFired++;
				OnFire();

				Log.Info( "Fired" );
			}

			ComputedForce = 0;
		}

		protected override void Fire()
		{
			var trace = new ArcTrace( Parent.EyePos, Parent.EyePos + Parent.EyeRot.Forward.Normal, ComputedForce, Turn.Instance?.WindForce ?? 0 ).Run();
		}
	}
}
