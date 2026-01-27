using Grubs.Common;
using Grubs.Pawn;
using Grubs.Systems.Pawn.Grubs;
using Grubs.Terrain;

namespace Grubs.Helpers;

[Title( "Grubs - Poison Helper" ), Category( "World" )]
public sealed class PoisonHelper : Component
{
	public static PoisonHelper Instance { get; set; }

	[Property] private GameObject FireObjectPrefab { get; set; }
	[Property] private float FireLifetime { get; set; } = 3f;
	[Property] private float MinParticleSpeed { get; set; } = 100f;

	[Sync] private NetList<FireParticle> PoisonParticles { get; set; } = new();
	private List<GameObject> PoisonObjects { get; set; } = new();

	public PoisonHelper()
	{
		Instance = this;
	}

	[Rpc.Owner]
	public void CreatePoison( FireParticle particle ) => PoisonParticles.Add( particle );

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if ( PoisonObjects.Count != PoisonParticles.Count )
		{
			if ( PoisonObjects.Count > PoisonParticles.Count )
			{
				PoisonObjects[0].Destroy();
				PoisonObjects.RemoveAt( 0 );
			}
			else
			{
				PoisonObjects.Add( FireObjectPrefab.Clone() );
			}
		}

		for ( var i = 0; i < PoisonObjects.Count; i++ )
		{
			if ( PoisonParticles.Count >= PoisonObjects.Count )
				PoisonObjects[i].WorldPosition = Vector3.Lerp( PoisonObjects[i].WorldPosition,
					PoisonParticles[i].Position,
					Time.Delta *
					Vector3.DistanceBetween( PoisonObjects[i].WorldPosition, PoisonParticles[i].Position ) );
		}

		for ( var i = 0; i < PoisonParticles.Count; i++ )
		{
			if ( PoisonParticles[i].TimeSinceCreated > FireLifetime )
			{
				PoisonParticles.RemoveAt( i );
				PoisonObjects[i].Destroy();
				PoisonObjects.RemoveAt( i );
				break;
			}

			ParticleTick( i );
		}
	}

	public void ParticleTick( int particle )
	{
		FireParticle fire = PoisonParticles[particle];

		if ( !IsProxy )
		{
			fire.TimeSinceCreated += Time.Delta;
			fire.TimeSinceLastDestruction += Time.Delta;

			fire.Position += PoisonParticles[particle].Velocity * Time.Delta * 5f;

			if ( fire.Velocity.Length > Instance.MinParticleSpeed )
				fire.Velocity *= 0.95f;

			fire.Velocity += Vector3.Up * 0.05f;
			PoisonParticles[particle] = fire;
		}

		var tr = Scene.Trace.Ray( PoisonParticles[particle].Position,
			PoisonParticles[particle].Position + PoisonParticles[particle].Velocity.Normal * 5f )
			.WithoutTags("player")
			.Run();

		if ( !tr.Hit )
			return;

		if ( !IsProxy )
		{
			fire.Velocity = Vector3.Reflect( PoisonParticles[particle].Velocity, tr.Normal );
			fire.Velocity *= 0.1f;
			//fire.Velocity += new Vector3( Game.Random.Float( -10f, 10f ), 0, 0 );

			PoisonParticles[particle] = fire;
		}

		fire.TimeSinceLastDestruction = 0f;
		PoisonParticles[particle] = fire;

		var gos = Scene.FindInPhysics( new Sphere( PoisonParticles[particle].Position, 35f ) );
		foreach ( var go in gos )
		{
			if ( go.Components.TryGet( out Grub grub, FindMode.EverythingInSelfAndAncestors ) )
				HandleGrubPoisoning( grub, PoisonParticles[particle].Position );
		}

		if ( IsProxy )
			return;
	}

	private void HandleGrubPoisoning( Grub grub, Vector3 position )
	{
		grub.SetPoisoned( true);
	}
}
