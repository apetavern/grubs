using TerryForm.Pawn;

namespace TerryForm.Weapons
{
	public class Grenade : Weapon
	{
		public override string WeaponName => "Grenade";
		public override string ModelPath => "models/weapons/grenade/grenade.vmdl";
		public override HoldPose HoldPose => HoldPose.Throwable;
		public override float PrimaryRate => 2f;

		public override void Fire()
		{
			new ExplodingProjectile()
				.WithRadius( 60 )
				.SetMaxBounces( 4 )
				.WithModel( ModelPath )
				.FireFrom( Owner.EyePos + Owner.EyeRot.Forward * 70, Owner.EyeRot.Forward.Normal, 50000 );

			base.Fire();
		}
	}
}
