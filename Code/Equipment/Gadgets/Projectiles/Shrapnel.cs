namespace Grubs.Equipment.Gadgets.Projectiles;

[Title( "Grubs - Shrapnel Spawner" ), Category( "Equipment" )]
public sealed class Shrapnel : Component
{
	[Property] public required ExplosiveProjectile Projectile { get; set; }

	[Property] public GameObject ShrapnelPrefab { get; set; }

	[Property] public int ShrapnelCount { get; set; }

	[Property] public float ShrapnelUpVelocity { get; set; } = 500f;
	[Property] public float ShrapnelSpreadVelocity { get; set; } = 150f;
	[Property] public float ShrapnelSpawnRandomness { get; set; } = 0f;
	[Property] public bool ShrapnelRandomizeRotation { get; set; } = false;

	protected override void OnStart()
	{
		Projectile.ProjectileExploded += SpawnShrapnel;
	}

	void SpawnShrapnel()
	{
		for ( var i = 0; i < ShrapnelCount; i++ )
		{
			var go = ShrapnelPrefab.Clone();
			go.WorldPosition = GameObject.WorldPosition + (Vector3.Random.WithY( 0 ) + Vector3.Up * 3f) * 2f;

			if ( ShrapnelRandomizeRotation )
			{
				go.WorldRotation = Rotation.From( Game.Random.Float( -180, 180 ), 0, 0 );
			}

			go.NetworkSpawn();
			if ( go.Components.TryGet( out Projectile pc ) && Components.TryGet( out Projectile pc2 ) )
			{
				if ( pc.Source.IsValid() && pc2.Source.IsValid() )
					pc.SourceId = pc2.SourceId;
			}

			var rb = go.Components.Get<Rigidbody>();
			if ( !rb.IsValid() )
				return;

			var startVelocity = (Vector3.Up * ShrapnelUpVelocity)
				.WithX( Game.Random.Float( -ShrapnelSpreadVelocity, ShrapnelSpreadVelocity ) );

			if ( ShrapnelSpawnRandomness > 0 )
			{
				startVelocity += Vector3.Random.Normal * ShrapnelSpawnRandomness;
			}

			rb.Velocity = startVelocity;
		}
	}
}
