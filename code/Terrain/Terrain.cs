using Grubs.Pawn;
using Grubs.Systems.Pawn.Grubs;
using Sandbox.Sdf;

namespace Grubs.Terrain;

[Title( "Grubs - Terrain" )]
public partial class GrubsTerrain : Component
{
	public static GrubsTerrain Instance { get; set; }

	[Property] public required Sdf2DWorld SdfWorld { get; set; }
	[Property] public required Water Water { get; set; }

	public int? SeedOverride { get; set; }

	public GrubsTerrain()
	{
		Instance = this;
	}

	public async void Init()
	{
		Game.SetRandomSeed( (int)(DateTime.Now - DateTime.UnixEpoch).TotalSeconds );

		if ( !SdfWorld.IsValid() )
		{
			Log.Error( "SdfWorld was null, this should never happen. Please report this issue." );
			Game.Close();
			return;
		}

		await Clear();

		await GameTask.MainThread();

		if ( SdfWorld.Transform is null )
		{
			Log.Error( "SdfWorld.Transform was null, this should never happen. Please report this issue." );
			Game.Close();
			return;
		}

		SdfWorld.WorldRotation = Rotation.FromRoll( 90f );

		if ( GrubsConfig.WorldTerrainType is GrubsConfig.TerrainType.Texture )
			SetupWorldFromTexture();
		else
			SetupGeneratedWorld();
	}

	public async Task Clear()
	{
		await SdfWorld.ClearAsync();
		WorldTextureLength = 0;
		WorldTextureHeight = 0;
	}

	public Vector3 FindSpawnLocation( bool inAir = false, float size = 16f, float maxAngle = 70f )
	{
		var retries = 0;
		var existingGrubs = Scene.GetAllComponents<Grub>().Select( g => g.GameObject );
		var existingDrops = Scene.GetAllObjects( true ).Where( go => go.IsRoot && go.Tags.Has( "drop" ) );
		var allAvoidances = existingGrubs.Concat( existingDrops ).ToList();
		var fallbackPosition = new Vector3();

		var maxWidth = GrubsConfig.TerrainLength;
		var maxHeight = GrubsConfig.TerrainHeight - 64;
		var minHeight = 60;

		var dist = 128f;
		const int maxRetries = 1000;

		while ( retries < maxRetries )
		{
			retries++;

			var randX = Game.Random.Int( maxWidth ) - maxWidth / 2;
			var randZ = Game.Random.Int( minHeight, maxHeight );
			var startPos = new Vector3( randX, 512, randZ );

			var tr = Scene.Trace.Ray( startPos, startPos + Vector3.Down * maxHeight )
				.WithAnyTags( "solid", "player" )
				.Size( size )
				.Run();

			if ( tr.Hit && !tr.StartedSolid )
			{
				var spawnPosition = inAir ? tr.StartPosition : tr.EndPosition;

				if ( tr.GameObject.Components.TryGet( out Grub _, FindMode.EverythingInSelfAndAncestors ) )
					continue;

				if ( PointInside( spawnPosition ) )
					continue;

				if ( Vector3.GetAngle( Vector3.Up, tr.Normal ) > maxAngle )
					continue;

				if ( spawnPosition.z < minHeight )
					continue;

				// If one object takes 200 tries to spawn, the rest of them should
				// assume that calculated distance is a good spot to start.
				dist = MathF.Min( dist, 128f - (128f * (retries / (float)maxRetries)) );
				if ( !IsDistanceValid( allAvoidances, spawnPosition, dist ) )
					continue;

				return spawnPosition;
			}
		}

		return fallbackPosition;
	}

	/// <summary>
	/// Given a list of GameObjects to check against, determine if a position is valid to spawn at.
	/// </summary>
	private bool IsDistanceValid( List<GameObject> objects, Vector3 pos, float minDistance )
	{
		bool validSpawn = true;

		foreach ( var go in objects )
		{
			if ( !go.IsValid() )
				continue;

			if ( pos.Distance( go.WorldPosition ) < minDistance )
				validSpawn = false;
		}

		return validSpawn;
	}

	public async Task LowerTerrain( float amount )
	{
		LowerTerrainEffects();
		
		var targetPosition = SdfWorld.WorldPosition - Vector3.Up * amount;

		while ( Vector3.DistanceBetween( SdfWorld.WorldPosition, targetPosition ) > Time.Delta * 5f )
		{
			var currentPosition = SdfWorld.WorldPosition;
			SdfWorld.WorldPosition = Vector3.Lerp( SdfWorld.WorldPosition, targetPosition, Time.Delta * 3f );

			foreach ( var grub in Scene.GetAllComponents<Grub>() )
			{
				grub.WorldPosition += Vector3.Up * (SdfWorld.WorldPosition.z - currentPosition.z);
			}
			
			await GameTask.DelaySeconds( Time.Delta / 2f );
		}
	}

	[Rpc.Broadcast( NetFlags.HostOnly )]
	private void LowerTerrainEffects()
	{
		Log.Info( "Playing sudden death terrain lowering effects." );
		
		Sound.Play( "suddendeath_rumble" );

		_ = ScreenShake();
	}

	private async Task ScreenShake()
	{
		await GameTask.MainThread();
			
		if ( !Scene.Camera.IsValid() )
			return;

		var progress = 0f;
		while ( progress < 1f )
		{
			progress += Time.Delta / 2f;
			Scene.Camera.WorldPosition += Vector3.Random * 6f;
			await GameTask.DelaySeconds( Time.Delta / 2f );
		}
	}

	protected override void DrawGizmos()
	{
		base.DrawGizmos();

		Gizmo.Transform = global::Transform.Zero;

		float l = GrubsConfig.TerrainLength;
		float h = GrubsConfig.TerrainHeight;
		Gizmo.Draw.LineBBox( new BBox( new Vector3( -l / 2f, 512f - 32f, 0f ), new Vector3( l / 2f, 512f + 96f, h ) ) );
	}

	public bool PointInside( Vector3 point )
	{
		var tr = Scene.Trace.Ray( point, point + Vector3.Right * 64f )
			.WithAnyTags( "solid" )
			.Size( 2f )
			.Run();

		return tr.Hit;
	}

	[ConCmd( "gr_set_seed" )]
	public static void SetSeedOverride( int seed )
	{
		Instance.SeedOverride = seed;
		Instance.Init();
	}
}
