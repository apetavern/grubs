using Grubs.Player;
using Grubs.Utils;
using Grubs.Weapons.Projectiles;

namespace Grubs.Weapons;

public class Bazooka : GrubWeapon
{
	public override string WeaponName => "Bazooka";
	public override string ModelPath => "models/weapons/bazooka/bazooka.vmdl";
	public override string ProjectileModelPath => "models/weapons/shell/shell.vmdl";
	public override FiringType FiringType => FiringType.Charged;
	public override HoldPose HoldPose => HoldPose.Bazooka;

	protected override void OnFire()
	{
		base.OnFire();

		// Projectile with bazooka shell, starts at bazooka location, and explodes after
		// 5 seconds if collision does not occur first.
		var segments = new ArcTrace( Parent, Parent.EyePosition )
			.RunTowards( Parent.EyeRotation.Forward.Normal, 0.5f * Charge, 0 );

		var projectile = new Projectile()
			.WithGrub( Parent as Grub )
			.WithModel( ProjectileModelPath )
			.SetPosition( Position )
			.MoveAlongTrace( segments )
			.WithSpeed( 1000 )
			.WithExplosionRadius( 100 );
		projectile.ExplodeAfterSeconds( 5f );
		GrubsCamera.SetTarget( projectile );
	}
}
