namespace Grubs.Equipment.Gadgets.Projectiles;

[Title( "Grubs - Homing Projectile" ), Category( "Equipment" )]
public class HomingProjectile : TargetedProjectile
{
	[Property] private float TimeBeforeHoming { get; set; } = 1f;
	[Property] private SoundEvent HomingLockSound { get; set; }

	public override void ShareData()
	{
		base.ShareData();
		StartHoming();
	}

	public async void StartHoming()
	{
		await Task.DelaySeconds( TimeBeforeHoming );
		if ( ProjectileMovement is not PhysicsProjectile pp )
			return;

		HomingEffects();

		pp.PhysicsBody.Gravity = false;
		pp.PhysicsBody.AngularDamping = 5f;
		pp.Model.SetBodyGroup( "flame", 1 );
		while ( Vector3.DistanceBetween( WorldPosition, ProjectileTarget ) > 10f )
		{
			await Task.FixedUpdate();
			pp.PhysicsBody.Velocity = WorldRotation.Forward * ProjectileSpeed;
			WorldRotation = Rotation.Lerp( WorldRotation, Rotation.LookAt( ProjectileTarget - WorldPosition ), 4f * Time.Delta );
		}
	}

	[Rpc.Broadcast]
	private void HomingEffects()
	{
		Sound.Play( HomingLockSound, WorldPosition );
	}
}
