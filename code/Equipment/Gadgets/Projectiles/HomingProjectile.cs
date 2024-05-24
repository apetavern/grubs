
namespace Grubs.Equipment.Gadgets.Projectiles;
public class HomingProjectile : Projectile
{
	public Vector3 ProjectileTarget { get; set; }
	[Property] Projectile ProjectileMovement { get; set; }

	public void PassDataOn()
	{
		ProjectileMovement.Source = Source;
		ProjectileMovement.Charge = Charge;
		StartHoming();
	}

	public async void StartHoming()
	{
		await Task.DelaySeconds( 1f );
		if(ProjectileMovement is PhysicsProjectile pp )
		{
			pp.PhysicsBody.Gravity = false;
			pp.PhysicsBody.AngularDamping = 5f;
			while ( Vector3.DistanceBetween(Transform.Position, ProjectileTarget) > 10f )
			{
				await Task.FixedUpdate();
				pp.PhysicsBody.Velocity = Transform.Rotation.Forward * ProjectileSpeed;
				Transform.Rotation = Rotation.Lerp(Transform.Rotation, Rotation.LookAt( ProjectileTarget - Transform.Position ), 2f * Time.Delta);
			}
		}
	}


}
