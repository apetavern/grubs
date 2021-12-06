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
		public override int MaxQuantityFired { get; set; } = 100;
		public override HoldPose HoldPose => HoldPose.Bazooka;

		// Weapon properties
		public Entity Projectile { get; set; }
		private float ComputedForce { get; set; } = 10;

		public override void Simulate( Client player )
		{
			if ( Input.Down( InputButton.Attack1 ) && WeaponEnabled && TimeSinceFired > SecondsBetweenFired )
			{
				ComputedForce += 0.4f;

				//ArcTrace.Draw( new ArcTrace( Parent.EyePos ).RunTowards( Parent.EyeRot.Forward.Normal, ComputedForce, Turn.Instance?.WindForce ?? 0 ) );
				ArcTrace.Draw( new ArcTrace( Parent.EyePos ).RunTo( Vector3.Zero ) );

				return;
			}

			if ( Input.Released( InputButton.Attack1 ) )
			{
				QuantityFired++;
				OnFire();
			}

			ComputedForce = 0;
		}

		protected override void Fire()
		{
			var trace = new ArcTrace( Parent.EyePos ).RunTowards( Parent.EyeRot.Forward.Normal, ComputedForce, Turn.Instance?.WindForce ?? 0 );

			new Projectile().MoveAlongTrace( trace ).WithModel( "models/weapons/shell/shell.vmdl" );
		}
	}
}
