using Sandbox.Utility;

namespace Grubs.Terrain;

public partial class GrubsTerrain
{
	[Sync] public int WorldTextureHeight { get; set; } = 0;
	[Sync] public int WorldTextureLength { get; set; } = 0;

	[Property] public Curve TerrainCurve { get; set; }
	public TimeSince LastChanged { get; set; } // Ugly fix for playtest. 5 second delay before the game can be started to allow terrain to load. 

	public void ResetTerrain()
	{
		WorldTextureLength = 0;
		WorldTextureHeight = 0;

		SdfWorld?.ClearAsync();
		SetupGeneratedWorld();
	}

	private void SetupGeneratedWorld()
	{
		var cfg = new MaterialsConfig( true, true );
		var materials = GetActiveMaterials( cfg );
		AddWorldBox( GrubsConfig.TerrainLength, GrubsConfig.TerrainHeight );

		WorldTextureLength = GrubsConfig.TerrainLength;
		WorldTextureHeight = GrubsConfig.TerrainHeight;

		GenerateWorld();
		LastChanged = 0f;
	}

	private bool[,] TerrainMap;
	private int[] NoiseMap;
	private float[,] DensityMap;
	private bool[,] BackgroundMap;

	private float amplitude => GrubsConfig.TerrainAmplitude;
	private float frequency => GrubsConfig.TerrainFrequency;

	private float noiseMin = 0.45f;
	private float noiseMax = 0.55f;
	private float noiseZoom => GrubsConfig.TerrainNoiseZoom;

	private float resolution = 8f;

	private int maxY;

	private void GenerateWorld()
	{
		var wLength = GrubsConfig.TerrainLength;
		var wHeight = GrubsConfig.TerrainHeight;

		var pointsX = (wLength / resolution).CeilToInt();
		var pointsY = (wHeight / resolution).CeilToInt();

		TerrainMap = new bool[pointsX, pointsY];
		NoiseMap = new int[pointsX];
		DensityMap = new float[pointsX, pointsY];
		BackgroundMap = new bool[pointsX, pointsY];

		var r = Game.Random.Int( 99999 );
		Log.Info( $"Seed: {r}" );

		for ( var x = 0; x < pointsX; x++ )
		{
			for ( var y = 0; y < pointsY; y++ )
			{
				TerrainMap[x, y] = false;
			}
		}

		// Generate Noise Map.
		maxY = 0;
		for ( var x = 0; x < pointsX; x++ )
		{
			NoiseMap[x] = (GetNoise( x + r, 0 ) * wHeight / 512f * TerrainCurve.Evaluate( (float)x / pointsX )).FloorToInt();
			if ( NoiseMap[x] >= pointsY )
				NoiseMap[x] = pointsY - 1;
			TerrainMap[x, NoiseMap[x]] = true;

			for ( var y = NoiseMap[x]; y >= 0; y-- )
			{
				maxY = Math.Max( maxY, y );
				TerrainMap[x, y] = true;
			}
		}

		WorldTextureHeight = (int)(maxY * resolution);

		// Subtract from the background.
		Array.Copy( TerrainMap, BackgroundMap, pointsX * pointsY );

		SubtractBackgroundBox( wLength, pointsX );

		// Populate Density Map for unique terrain features.
		var fgMaterials = GetActiveMaterials( MaterialsConfig.Default );

		var toSubtract = new List<Vector2>();

		for ( var x = 0; x < pointsX; x++ )
		{
			for ( var y = 0; y < maxY; y++ )
			{
				DensityMap[x, y] = Noise.Simplex( (x + r) / noiseZoom, (y + r) / noiseZoom );

				if ( DensityMap[x, y] > noiseMin && DensityMap[x, y] < noiseMax )
				{
					TerrainMap[x, y] = false;
				}

				// Subtract from the world in passing.
				if ( !TerrainMap[x, y] )
				{
					var midpoint = new Vector2( x * resolution, y * resolution );
					toSubtract.Add( midpoint );
				}
			}
		}

		ShuffleSubtractList( toSubtract );

		foreach ( var midpoint in toSubtract )
		{
			// TODO: figure out best way to subtract
			SubtractCircle( midpoint, 8f, 0, true );
		}

		SubtractBackground( pointsX );
	}

	private void SubtractBackgroundBox( int wLength, int pointsY )
	{
		var bb = new Vector2( 0, maxY * resolution );
		var aa = new Vector2( wLength, pointsY * resolution );
		SubtractBox( bb, aa, 2, worldOffset: true );
	}

	private void SubtractBackground( int pointsX )
	{
		var materialsConfig = new MaterialsConfig( false, true );
		var bgMaterials = GetActiveMaterials( materialsConfig );

		var toSubtract = new List<Vector2>();

		for ( var x = 0; x < pointsX; x++ )
		{
			for ( var y = 0; y < maxY + 1; y++ )
			{
				if ( !BackgroundMap[x, y] )
				{
					var midpoint = new Vector2( x * resolution, y * resolution );
					toSubtract.Add( midpoint );
				}
			}
		}

		ShuffleSubtractList( toSubtract );

		foreach ( var midpoint in toSubtract )
		{
			SubtractCircle( midpoint, 8f, 2, true );
		}
	}

	private static void ShuffleSubtractList( List<Vector2> list )
	{
		var random = Random.Shared;

		for ( var i = 0; i < list.Count; ++i )
		{
			var j = random.Next( i, list.Count - 1 );
			(list[i], list[j]) = (list[j], list[i]);
		}
	}

	private void SubtractForeground( int pointsX )
	{
		var fgMaterials = GetActiveMaterials( MaterialsConfig.Default );
		for ( var x = 0; x < pointsX; x++ )
		{
			for ( var y = 0; y < maxY; y++ )
			{
				if ( !TerrainMap[x, y] )
				{
					var midpoint = new Vector2( x * resolution, y * resolution );
					SubtractCircle( midpoint, 8f, 0, true );
				}
			}
		}
	}

	private float GetNoise( int x, int y )
	{
		return amplitude * Noise.Perlin( x * frequency, y * frequency );
	}

	[ConCmd( "gr_regen_terrain" )]
	public static void RegenerateTerrain()
	{
		if ( !Game.IsEditor && !Networking.IsHost )
			return;

		Instance.ResetTerrain();
	}
}
