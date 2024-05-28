using Grubs.Equipment.Gadgets.Projectiles;
using Grubs.Pawn;
using Sandbox;

namespace Grubs.Equipment.Weapons;

public sealed class RemoteDetonateWeapon : Weapon
{
	private ExplosiveProjectile Projectile { get; set; }

	private bool _projectileExploded;

	protected override void HandleComplexFiringInput()
	{
		base.HandleComplexFiringInput();

		if ( _projectileExploded )
		{
			FireFinished();
			return;
		}

		if ( Projectile is null || !Projectile.IsValid() )
			return;

		GrubFollowCamera.Local.SetTarget( Projectile.GameObject );

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

			Projectile.ProjectileExploded += () => _projectileExploded = true;
		}
	}

	protected override void FireFinished()
	{
		base.FireFinished();
		_projectileExploded = false;
		Projectile = null;
		ForceHideWeapon = false;
	}

	public void ReceiveProjectile( GameObject ProjectileObject )
	{
		Projectile = ProjectileObject.Components.Get<ExplosiveProjectile>();
	}
}
