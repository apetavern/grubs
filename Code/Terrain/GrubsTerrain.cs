using System.Text;
using Grubs.Drops;
using Grubs.Equipment.Gadgets.Projectiles;
using Grubs.Pawn;
using Grubs.Systems.Pawn.Grubs;
using Sandbox.Sdf;
using Sandbox.Utility;

namespace Grubs.Terrain;

[Title( "Grubs - Legacy Terrain" )]
public partial class GrubsTerrain : Component
{
	public static GrubsTerrain Instance { get; set; }
	
	[Property] public Material CloudMaterial { get; set; }

	[Property] public required Sdf2DWorld SdfWorld { get; set; }
	[Property] public required Water Water { get; set; }

	public int? SeedOverride { get; set; }

	public GrubsTerrain()
	{
		Instance = this;
	}

	protected override void OnStart()
	{
		base.OnStart();

		// var textureColor = CloudMaterial.GetTexture( "g_tColor" );
		// var textureNormal = CloudMaterial.GetTexture( "g_tNormal" );
		// var textureRoughness = CloudMaterial.GetTexture( "TextureRoughness" );
		// var textureAO = CloudMaterial.GetTexture( "g_tAmbientOcclusion" );
		//
		// ApplyTextureToMaterials( "g_tColour", textureColor );
		// ApplyTextureToMaterials( "g_tNormal", textureNormal );
		// ApplyTextureToMaterials( "g_tRough", textureRoughness );
		// ApplyTextureToMaterials( "g_tAO", textureAO );
		//
		// ApplyScorchColor( Color.Black.Lighten( 0.2f ) );
	}

	private void ApplyTextureToMaterials( string attributeName, Texture texture )
	{
		SandMaterial.FrontFaceMaterial.Set( attributeName, texture );
		SandMaterial.BackFaceMaterial.Attributes.Set( attributeName, texture );
		SandMaterial.CutFaceMaterial.Attributes.Set( attributeName, texture );
	}

	private void ApplyScorchColor( Color color )
	{
		SandMaterial.FrontFaceMaterial.Set( "g_vScorchTint_Colour", color );
		SandMaterial.BackFaceMaterial.Attributes.Set( "g_vScorchTint_Colour", color );
		SandMaterial.CutFaceMaterial.Attributes.Set( "g_vScorchTint_Colour", color );
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

		await Task.MainThread();

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

	[ConCmd( "gr_save_terrain" )]
	public static void SaveTerrainCmd( string name )
	{
		Instance.SerializeTerrain( name );
	}

	public void SerializeTerrain( string name )
	{
		Log.Info( $"Attempting to serialize terrain from {SdfWorld.GameObject.Name}..." );
		Log.Info( $"SdfWorld has {SdfWorld.ModificationCount} modifications." );

		try
		{
			var byteStream = ByteStream.Create( 512 );
			var mods = SdfWorld.Write( ref byteStream, 0 );
			Log.Info( $"Writing {mods} modifications to stream..." );
			var stream = FileSystem.Data.OpenWrite( $"terrain_{name}.json" );
			stream.Write( byteStream.ToArray() );
			stream.Close();
			
			Log.Info( $"Done writing to terrain_{name}.json" );
		}
		catch ( Exception e )
		{
			Log.Error( e );
		}
	}

	[ConCmd( "gr_load_terrain" )]
	public static void LoadTerrainCmd( string name )
	{
		Instance.DeserializeTerrain( name );
	}

	public void DeserializeTerrain( string name )
	{
		Log.Info( $"Attempting to load terrain for {SdfWorld.GameObject.Name}..." );
		var file = $"terrain_{name}.json";
		
		_ = SdfWorld.ClearAsync();
		
		if ( !FileSystem.Data.FileExists( file ) )
		{
			Log.Warning( $"Failed to load terrain. File does not exist." );
		}

		_ = ReadExistingTerrain( name );
	}

	private async Task ReadExistingTerrain( string name )
	{
		await SdfWorld.ClearAsync();
		
		try
		{
			var contents = FileSystem.Data.ReadAllBytes( $"terrain_{name}.json" );
			var byteStream = ByteStream.CreateReader( contents );

			SdfWorld.ClearAndReadData( ref byteStream );
		}
		catch ( Exception e )
		{
			Log.Error( e );
		}
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

			var positionOffset = Vector3.Up * (SdfWorld.WorldPosition.z - currentPosition.z);
			foreach ( var grub in Scene.GetAllComponents<Grub>() )
			{
				grub.WorldPosition += positionOffset;
			}

			foreach ( var crate in Scene.GetAllComponents<Crate>() )
			{
				crate.WorldPosition += positionOffset;
			}

			foreach ( var projectile in Scene.GetAllComponents<PhysicsProjectile>() )
			{
				projectile.WorldPosition += positionOffset;
			}

			foreach ( var destructibleObject in Scene.GetAllComponents<DestructibleObject>() )
			{
				destructibleObject.WorldPosition += positionOffset;
			}

			foreach ( var grave in Scene.GetAllObjects( true ).Where( go => go.Tags.Has( "grave" ) ) )
			{
				grave.WorldPosition += positionOffset;
			}
			
			await Task.DelaySeconds( Time.Delta / 2f );
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
		await Task.MainThread();
			
		if ( !Scene.Camera.IsValid() )
			return;

		var progress = 0f;
		while ( progress < 1f )
		{
			progress += Time.Delta / 2f;
			Scene.Camera.WorldPosition += Vector3.Random * 6f;
			await Task.DelaySeconds( Time.Delta / 2f );
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
