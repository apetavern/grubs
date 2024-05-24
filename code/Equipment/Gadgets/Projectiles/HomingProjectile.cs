
using Grubs.Equipment.Weapons;
using System.ComponentModel.Design;

namespace Grubs.Equipment.Gadgets.Projectiles;

[Title( "Grubs - Homing Projectile" ), Category( "Equipment" )]
public class HomingProjectile : Projectile
{
	public Vector3 ProjectileTarget { get; set; }
	[Property] private Projectile ProjectileMovement { get; set; }
	[Property] private float TimeBeforeHoming { get; set; } = 1f;

	public void ShareData()
	{
		if ( Source != null )
		{
			ProjectileMovement.Source = Source;
			ProjectileMovement.Charge = Charge;
		}
		else{
			Source = ProjectileMovement.Source;
			Charge = ProjectileMovement.Charge;
		}
		ProjectileTarget = Source.Components.Get<HomingWeapon>().ProjectileTarget;
		StartHoming();
	}

	public async void StartHoming()
	{
		await Task.DelaySeconds( TimeBeforeHoming );
		if ( ProjectileMovement is not PhysicsProjectile pp )
			return;

		pp.PhysicsBody.Gravity = false;
		pp.PhysicsBody.AngularDamping = 5f;
		pp.Model.SetBodyGroup( "flame", 1 );
		while ( Vector3.DistanceBetween(Transform.Position, ProjectileTarget) > 10f )
		{
			await Task.FixedUpdate();
			pp.PhysicsBody.Velocity = Transform.Rotation.Forward * ProjectileSpeed;
			Transform.Rotation = Rotation.Lerp(Transform.Rotation, Rotation.LookAt( ProjectileTarget - Transform.Position ), 2f * Time.Delta);
		}
		
	}

}
