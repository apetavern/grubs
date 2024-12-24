using Grubs.Common;
using Grubs.Pawn;
using Grubs.Systems.Pawn.Grubs;
using Grubs.Terrain;

namespace Grubs.Helpers;

[Title( "Grubs - Fire Helper" ), Category( "World" )]
public sealed class FireHelper : Component
{
	public static FireHelper Instance { get; set; }

	[Property] private GameObject FireObjectPrefab { get; set; }
	[Property] private float FireLifetime { get; set; } = 3f;
	[Property] private float MinParticleSpeed { get; set; } = 100f;

	[Sync] private NetList<FireParticle> FireParticles { get; set; } = new();
	private List<GameObject> FireObjects { get; set; } = new();

	public FireHelper()
	{
		Instance = this;
	}

	[Authority]
	public void CreateFire( FireParticle particle ) => FireParticles.Add( particle );

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if ( FireObjects.Count != FireParticles.Count )
		{
			if ( FireObjects.Count > FireParticles.Count )
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
			if ( FireParticles.Count >= FireObjects.Count )
				FireObjects[i].WorldPosition = Vector3.Lerp( FireObjects[i].WorldPosition,
					FireParticles[i].Position,
					Time.Delta *
					Vector3.DistanceBetween( FireObjects[i].WorldPosition, FireParticles[i].Position ) );
		}

		for ( var i = 0; i < FireParticles.Count; i++ )
		{
			if ( FireParticles[i].TimeSinceCreated > FireLifetime )
			{
				FireParticles.RemoveAt( i );
				FireObjects[i].Destroy();
				FireObjects.RemoveAt( i );
				break;
			}

			ParticleTick( i );
		}
	}

	public void ParticleTick( int particle )
	{
		FireParticle fire = FireParticles[particle];

		if ( !IsProxy )
		{
			fire.TimeSinceCreated += Time.Delta;
			fire.TimeSinceLastDestruction += Time.Delta;

			fire.Position += FireParticles[particle].Velocity * Time.Delta * 5f;

			if ( fire.Velocity.Length > Instance.MinParticleSpeed )
				fire.Velocity *= 0.95f;

			fire.Velocity += Vector3.Down;
			FireParticles[particle] = fire;
		}

		var tr = Scene.Trace.Ray( FireParticles[particle].Position,
			FireParticles[particle].Position + FireParticles[particle].Velocity.Normal * 5f )
			.WithoutTags("player")
			.Run();

		if ( !tr.Hit )
			return;

		if ( !IsProxy )
		{
			fire.Velocity = Vector3.Reflect( FireParticles[particle].Velocity, tr.Normal );
			fire.Velocity *= 0.1f;
			fire.Velocity += new Vector3( Game.Random.Float( -10f, 10f ), 0, 0 );

			FireParticles[particle] = fire;
		}

		if ( fire.TimeSinceLastDestruction < 0.25f )
			return;

		fire.TimeSinceLastDestruction = 0f;
		FireParticles[particle] = fire;

		var gos = Scene.FindInPhysics( new Sphere( FireParticles[particle].Position, 10f ) );
		foreach ( var go in gos )
		{
			if ( go.Components.TryGet( out Grub grub, FindMode.EverythingInSelfAndAncestors ) )
				HandleGrubExplosion( grub, FireParticles[particle].Position );

			if ( !go.Components.TryGet( out Health health, FindMode.EverythingInSelfAndAncestors ) )
				continue;

			health.TakeDamage(
				GrubsDamageInfo.FromFire( 0.75f, grub?.Id ?? Guid.Empty, grub?.Name ?? string.Empty, worldPosition: FireParticles[particle].Position ) );
		}

		if ( IsProxy )
			return;

		const float torchSize = 6f;
		var startPos = FireParticles[particle].Position;
		var endPos = FireParticles[particle].Position + FireParticles[particle].Velocity.Normal * torchSize;

		using ( Rpc.FilterInclude( c => c.IsHost ) )
		{
			GrubsTerrain.Instance.SubtractLine( new Vector2( startPos.x, startPos.z ),
				new Vector2( endPos.x, endPos.z ), torchSize, 1 );
			GrubsTerrain.Instance.ScorchLine( new Vector2( startPos.x, startPos.z ),
				new Vector2( endPos.x, endPos.z ),
				torchSize + 8f );
		}
	}

	private void HandleGrubExplosion( Grub grub, Vector3 position )
	{
		var dir = (grub.WorldPosition - position).Normal;
		dir = dir.WithY( 0f );

		grub.CharacterController.Punch( dir * 16f );
		grub.CharacterController.ReleaseFromGround();
	}
}

public struct FireParticle
{
	public Vector3 Position { get; set; }
	public Vector3 Velocity { get; set; }
	public float TimeSinceCreated { get; set; }
	public float TimeSinceLastDestruction { get; set; }
}
