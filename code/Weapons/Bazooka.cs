using TerryForm.Pawn;

namespace TerryForm.Weapons
{
	public class Bazooka : Weapon
	{
		public override string WeaponName => "Bazooka";
		public override string ModelPath => "models/weapons/bazooka/bazooka.vmdl";
		public override HoldPose HoldPose => HoldPose.Bazooka;
		public override float PrimaryRate => 2f;

		public override void Fire()
		{
			new ExplodingProjectile()
				.WithRadius( 60 )
				.SetMaxBounces( 1 )
				.WithModel( "models/weapons/shell/shell.vmdl" )
				.FireFrom( Owner.EyePos + Owner.EyeRot.Forward * 70, Owner.EyeRot.Forward.Normal, 50000 );
		}
	}
}
