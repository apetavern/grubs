using Grubs.Equipment.Weapons;
using Grubs.Helpers;

namespace Grubs.Equipment.Gadgets.Projectiles;

internal class FireEmitter : Component
{
	[Property] public required ExplosiveProjectile Projectile { get; set; }

	[Property] private int FireParticleCount { get; set; } = 5;

	[Property] private float InitialUpVelocity { get; set; } = 200f;

	[Property] private float LeftRightVelocityRandom { get; set; } = 200f;

	[Property] private bool SphericalEmission { get; set; } = false;

	[Property] public Weapon Weapon { get; set; }

	protected override void OnStart()
	{
		if ( Projectile != null )
			Projectile.ProjectileExploded += SpawnFire;

		if ( Weapon != null )
			Weapon.OnFire += SpawnFire;
	}


	public void SpawnFire()
	{
		List<Vector3> direction = new List<Vector3>() { Vector3.Up, Vector3.Forward, Vector3.Backward, Vector3.Down };
		var closestSurfaceNormal = Vector3.Up;
		var dist = float.MaxValue;
		foreach ( var dir in direction )
		{
			var tr = Scene.Trace.Ray( WorldPosition, WorldPosition + dir * Projectile.ExplosionRadius * 2f )
				.Run();
			if ( tr.Hit && tr.Distance < dist )
			{
				dist = tr.Distance;
				closestSurfaceNormal = tr.Normal;
			}
		}

		var addedPhysicsVelocity = Components.TryGet( out Rigidbody rb ) ? rb.Velocity * Time.Delta : Vector3.Zero;

		for ( int i = 0; i < FireParticleCount; i++ )
		{
			FireParticle particle = new FireParticle()
			{
				Position = WorldPosition,
				Velocity = !SphericalEmission ? (new Vector3( Game.Random.Float( -LeftRightVelocityRandom, LeftRightVelocityRandom ), 0,
								InitialUpVelocity +
								(Game.Random.Float( -InitialUpVelocity, InitialUpVelocity ) / 3f) ) *
							Rotation.LookAt( closestSurfaceNormal, Vector3.Up )) + addedPhysicsVelocity :
							new Vector3( Game.Random.Float( -LeftRightVelocityRandom, LeftRightVelocityRandom ), 0,
								InitialUpVelocity +
								(Game.Random.Float( -LeftRightVelocityRandom, LeftRightVelocityRandom ) / 3f) ) + addedPhysicsVelocity,
				TimeSinceCreated = 0f,
				TimeSinceLastDestruction = 0f
			};
			FireHelper.Instance.CreateFire( particle );
		}
	}

	public void SpawnFire( int charge )
	{
		for ( int i = 0; i < FireParticleCount; i++ )
		{
			FireParticle particle = new FireParticle()
			{
				Position = Weapon.GetStartPosition(),
				Velocity =
					new Vector3( Game.Random.Float( -LeftRightVelocityRandom, LeftRightVelocityRandom ), 0,
						InitialUpVelocity ) * Weapon.Equipment.Grub.PlayerController.LookAngles.ToRotation() *
					Rotation.FromPitch( 90 * Weapon.Equipment.Grub.PlayerController.Facing ),
				TimeSinceCreated = 0f,
				TimeSinceLastDestruction = 0f
			};
			FireHelper.Instance.CreateFire( particle );
		}
	}
}
