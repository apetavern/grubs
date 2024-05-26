using Grubs.Equipment.Gadgets.Projectiles;
using Grubs.Pawn;
using Sandbox;

namespace Grubs.Equipment.Weapons;

public sealed class RemoteDetonateWeapon : Weapon
{
	private ExplosiveProjectile Projectile { get; set; }

	bool ProjectileExploded;

	protected override void HandleComplexFiringInput()
	{
		base.HandleComplexFiringInput();

		if ( ProjectileExploded )
		{
			FireFinished();
			return;
		}

		if ( Projectile != null )
		{
			GrubFollowCamera.Local.SetTarget( Projectile.GameObject );
		}

		if ( Input.Pressed( "fire" ) && IsFiring && Projectile != null )
		{
			Projectile.Explode();
			FireFinished();
			return;
		}

		if ( Input.Pressed( "fire" ) && !IsFiring )
		{
			IsFiring = true;
			ForceHideWeapon = true;
			if ( OnFire is not null )
				OnFire.Invoke( 100 );
			else
				FireImmediate();

			Projectile.ProjectileExploded += () => ProjectileExploded = true;
		}
	}

	protected override void FireFinished()
	{
		base.FireFinished();
		ProjectileExploded = false;
		Projectile = null;
		ForceHideWeapon = false;
	}

	public void ReceiveProjectile( GameObject ProjectileObject )
	{
		Projectile = ProjectileObject.Components.Get<ExplosiveProjectile>();
	}
}
