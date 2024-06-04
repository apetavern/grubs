using Grubs.Common;
using Grubs.Pawn;
using Grubs.Terrain;

namespace Grubs.Helpers;

[Title( "Grubs - Fire Helper" ), Category( "World" )]
public sealed class FireHelper : Component
{
	public static FireHelper Instance { get; set; }

	[Property] private float FireLifetime { get; set; } = 3f;
	[Property] public float MinParticleSpeed { get; set; } = 100f;

	public FireHelper()
	{
		Instance = this;
	}

	[Sync] public List<Vector3> FireParticlePositions { get; set; } = new();
	[Sync] public List<Vector3> FireParticleVelocities { get; set; } = new();
	[Sync] public List<float> FireParticleLifetimes { get; set; } = new();
	[Sync] public List<float> LastDestructionTime { get; set; } = new();

	[Property] private List<GameObject> FireObjects { get; set; } = new();

	[Property] private GameObject FireObjectPrefab { get; set; }

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if ( FireObjects.Count != FireParticlePositions.Count )
		{
			if ( FireObjects.Count > FireParticlePositions.Count )
			{
				FireObjects[0].Destroy();
				FireObjects.RemoveAt( 0 );
			}
			else
			{
				FireObjects.Add( FireObjectPrefab.Clone() );
			}
		}

		for ( var i = 0; i < FireObjects.Count; i++ )
		{
			if ( FireParticlePositions.Count >= FireObjects.Count )
				FireObjects[i].Transform.Position = Vector3.Lerp( FireObjects[i].Transform.Position,
					FireParticlePositions[i],
					Time.Delta *
					Vector3.DistanceBetween( FireObjects[i].Transform.Position, FireParticlePositions[i] ) );
		}

		if ( IsProxy ) return;

		for ( var i = 0; i < FireParticlePositions.Count; i++ )
		{
			if ( FireParticleLifetimes[i] > FireLifetime )
			{
				FireParticlePositions.RemoveAt( i );
				FireParticleVelocities.RemoveAt( i );
				FireParticleLifetimes.RemoveAt( i );
				LastDestructionTime.RemoveAt( i );
				FireObjects[i].Destroy();
				FireObjects.RemoveAt( i );
				break;
			}

			ParticleTick( i );
		}
	}

	public void ParticleTick( int particle )
	{
		FireParticleLifetimes[particle] += Time.Delta;
		FireParticlePositions[particle] += FireParticleVelocities[particle] * Time.Delta * 5f;

		if ( FireParticleVelocities[particle].Length > Instance.MinParticleSpeed )
			FireParticleVelocities[particle] *= 0.95f;

		FireParticleVelocities[particle] += Vector3.Down;

		var tr = Scene.Trace.Ray( FireParticlePositions[particle],
			FireParticlePositions[particle] + FireParticleVelocities[particle].Normal * 5f ).Run();

		if ( !tr.Hit )
			return;

		FireParticleLifetimes[particle] += Time.Delta;
		FireParticleVelocities[particle] = Vector3.Reflect( FireParticleVelocities[particle], tr.Normal );
		FireParticleVelocities[particle] *= 0.1f;
		FireParticleVelocities[particle] += new Vector3( Game.Random.Float( -10f, 10f ), 0, 0 );

		if ( !(Time.Now - LastDestructionTime[particle] > 0.25f) )
			return;

		var gos = Scene.FindInPhysics( new Sphere( FireParticlePositions[particle], 10f ) );
		foreach ( var go in gos )
		{
			if ( go.Components.TryGet( out Grub grub, FindMode.EverythingInSelfAndAncestors ) )
				HandleGrubExplosion( grub, FireParticlePositions[particle] );

			if ( !go.Components.TryGet( out Health health, FindMode.EverythingInSelfAndAncestors ) )
				continue;

			health.TakeDamage(
				GrubsDamageInfo.FromFire( 0.75f, grub, worldPosition: FireParticlePositions[particle] ) );
		}

		const float torchSize = 6f;
		var startPos = FireParticlePositions[particle];
		var endPos = FireParticlePositions[particle] + FireParticleVelocities[particle].Normal * torchSize;

		using ( Rpc.FilterInclude( c => c.IsHost ) )
		{
			GrubsTerrain.Instance.SubtractLine( new Vector2( startPos.x, startPos.z ),
				new Vector2( endPos.x, endPos.z ), torchSize, 1 );
			GrubsTerrain.Instance.ScorchLine( new Vector2( startPos.x, startPos.z ),
				new Vector2( endPos.x, endPos.z ),
				torchSize + 8f );
		}

		LastDestructionTime[particle] = Time.Now;
	}

	private void HandleGrubExplosion( Grub grub, Vector3 position )
	{
		var dir = (grub.Transform.Position - position).Normal;
		dir = dir.WithY( 0f );

		grub.CharacterController.Punch( dir * 16f );
		grub.CharacterController.ReleaseFromGround();
	}

	[Broadcast]
	public void CreateFire( FireParticle particle )
	{
		if ( IsProxy ) return;

		FireParticlePositions.Add( particle.Position );
		FireParticleVelocities.Add( particle.Velocity );
		FireParticleLifetimes.Add( 0f );
		LastDestructionTime.Add( Time.Now - 1f );
	}
}

public struct FireParticle
{
	public Vector3 Position { get; set; }
	public Vector3 Velocity { get; set; }
	public TimeSince TimeSinceCreated { get; set; }
}
