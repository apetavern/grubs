using Grubs.Player;
using Grubs.Utils;
using Grubs.Weapons.Projectiles;

namespace Grubs.Weapons;

public class Grenade : GrubWeapon
{
	public override string WeaponName => "Grenade";
	public override string ModelPath => "models/weapons/grenade/grenade.vmdl";
	public override string ProjectileModelPath => "models/weapons/grenade/grenade.vmdl";
	public override FiringType FiringType => FiringType.Charged;
	public override HoldPose HoldPose => HoldPose.Throwable;
	public override bool HasReticle => true;

	protected override void OnFire()
	{
		base.OnFire();

		var segments = new ArcTrace( Parent, Parent.EyePosition )
			.RunTowardsWithBounces( Parent.EyeRotation.Forward.Normal, 0.4f * Charge, 0, maxBounceQty: 5 );

		var projectile = new Projectile()
		   .WithGrub( Parent as Grub )
		   .WithModel( ProjectileModelPath )
		   .SetPosition( Position )
		   .MoveAlongTrace( segments )
		   .WithSpeed( 1000 )
		   .WithExplosionRadius( 100 )
		   .WithCollisionExplosionDelay( 3f );
		GrubsCamera.SetTarget( projectile );
	}
}
