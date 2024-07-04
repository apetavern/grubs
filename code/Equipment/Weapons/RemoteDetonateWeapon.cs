using Grubs.Equipment.Gadgets.Projectiles;
using Grubs.Pawn;
using Grubs.UI;

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

		if ( Projectile is not null && Projectile.IsValid() )
			GrubFollowCamera.Local.SetTarget( Projectile.GameObject );

		if ( Input.Pressed( "fire" ) && IsFiring && Projectile != null )
		{
			FireFinished();

			Projectile.Explode();

			_projectileExploded = false;
			Projectile = null;
			ForceHideWeapon = false;

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

			Projectile.ProjectileExploded += () =>
			{
				_projectileExploded = true;
				WeaponInfoPanel?.GameObject.Destroy();
			};
		}
	}

	public void ReceiveProjectile( GameObject ProjectileObject )
	{
		Projectile = ProjectileObject.Components.Get<ExplosiveProjectile>();

		if ( WeaponInfoPanel is null )
			return;

		WeaponInfoPanel.Target = ProjectileObject;
		WeaponInfoPanel.Inputs = new Dictionary<string, string>()
		{
			{ "fire", "Detonate" }
		};
	}
}
