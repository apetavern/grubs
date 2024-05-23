using Sandbox;

namespace Grubs.Equipment.Weapons;

[Title( "Grubs - Shrapnel Spawner" ), Category( "Equipment" )]
public sealed class ShrapnelSpawnComponent : Component
{
	[Property] public required ExplosiveProjectileComponent Projectile { get; set; }

	[Property] public GameObject ShrapnelPrefab { get; set; }

	[Property] public int ShrapnelCount { get; set; }

	[Property] public float ShrapnelUpVelocity { get; set; } = 500f;

	[Property] public float ShrapnelSpreadVelocity { get; set; } = 150f;

	protected override void OnStart()
	{
		Projectile.ProjectileExploded += SpawnShrapnel;
	}

	void SpawnShrapnel()
	{
		for ( var i = 0; i < ShrapnelCount; i++ )
		{
			var go = ShrapnelPrefab.Clone();
			go.Transform.Position = GameObject.Transform.Position + (Vector3.Random.WithY( 0 ) + Vector3.Up * 3f) * 2f;
			go.NetworkSpawn();
			if ( go.Components.TryGet( out ProjectileComponent pc ) && Components.TryGet( out ProjectileComponent pc2 ) )
			{
				pc.Source = pc2.Source;
			}

			var rb = go.Components.Get<Rigidbody>();
			if ( rb is null )
				return;
			var startVelocity = (Vector3.Up * ShrapnelUpVelocity)
				.WithX( Game.Random.Float( -ShrapnelSpreadVelocity, ShrapnelSpreadVelocity ) );
			rb.Velocity = startVelocity;
		}
	}
}
