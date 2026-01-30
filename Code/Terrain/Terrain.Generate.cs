using Grubs.Extensions;
using Sandbox.Sdf;
using Sandbox.Utility;

namespace Grubs.Terrain;

public partial class GrubsTerrain
{
	[Sync] public int WorldTextureHeight { get; set; } = 0;
	[Sync] public int WorldTextureLength { get; set; } = 0;

	[Property] public Curve TerrainCurve { get; set; }

	public void ResetTerrain()
	{
		WorldTextureLength = 0;
		WorldTextureHeight = 0;

		SdfWorld?.ClearAsync();
		SetupGeneratedWorld();
	}

	private void SetupGeneratedWorld()
	{
		WorldTextureLength = GrubsConfig.TerrainLength;
		WorldTextureHeight = GrubsConfig.TerrainHeight;

		// Choose between hillside terrain or cavern terrain
		if ( GrubsConfig.WorldTerrainType == GrubsConfig.TerrainType.Cavern ) // You'll need to add this config option
			CreateCavernTerrain();
		else
			CreateNoiseMap();
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

	private void GenerateWorldTextureSdf()
	{
		var worldLength = GrubsConfig.TerrainLength;
		var worldHeight = GrubsConfig.TerrainHeight;
		
		var random = Game.Random.Int( 99999 );

	}

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
		if ( SeedOverride != null )
			r = SeedOverride.Value;
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
			NoiseMap[x] = (GetNoise( x + r, 0 ) * wHeight / 512f * TerrainCurve.Evaluate( (float)x / pointsX ))
				.FloorToInt();
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
	
	// public static float[] GenerateHeights( float worldHeight )
	// {
	// 	var heights = new float[(int)worldHeight + 1];
	// 	for (var i = 0; i < heights.Length; i++)
	// 	{
	// 		heights[i] = Noise.Perlin( Game.Random.Float() * worldHeight );
	// 	}
	//
	// 	return heights;
	// }
}

// file record HeightMap( Curve HeightCurve, float WorldLength, float WorldHeight, float Random ) : INoiseField
// {
// 	private readonly float[] _heights = GrubsTerrain.GenerateHeights( WorldHeight );
// 	
// 	public float Sample( float x, float y )
// 	{
// 		x = Math.Clamp( x, 0, WorldLength );
// 		var index = x;
// 		var leftIndex = (int)index;
// 		var rightIndex = Math.Min( leftIndex + 1, _heights.Length - 1 );
//
// 		var fraction = index - leftIndex;
// 		var height = MathX.Lerp( _heights[leftIndex], _heights[rightIndex], fraction );
// 		return y <= height ? 1f : 0f;
// 	}
//
// 	public float Sample( float x, float y, float z )
// 	{
// 		throw new NotImplementedException();
// 	}
// }

// file record SimplexNoiseField( Curve Curve, float TerrainLength, float TerrainHeight, float Amplitude, float Frequency ) : INoiseField
// {
// 	public float Sample( float x, float y )
// 	{
// 		// var realX = (x + TerrainLength / 2f).Clamp( 0, TerrainLength);
// 		// var eval = Curve.Evaluate( realX / TerrainLength );
// 		// Log.Info( $"Curve Evaluate: {eval}" );
// 		// var baseValue = (1f - eval) * baseNoise.Sample( x, y );
// 		// return baseValue;
// 		
// 		var noise = Amplitude * Noise.Perlin( x * Frequency, y * Frequency );
// 		Log.Info( noise );
// 		return noise * TerrainLength / 512f;
// 	}
// 	
// 	public float Sample( float x, float y, float z )
// 	{
// 		throw new NotImplementedException();
// 	}
// }
		
// var heightMap = new HeightMap( TerrainCurve, worldLength, worldHeight, random );
// var terrainSdf = new NoiseSdf2D(
// 	new Vector2( -worldLength / 2f, 0 ), 
// 	new Vector2( worldLength / 2f, worldHeight ), 
// 	heightMap
// );
// var cfg = new MaterialsConfig( true, true );
// var materials = GetActiveMaterials( cfg );
// Add( SdfWorld, terrainSdf, materials.ElementAt( 0 ).Key );

// var simplexNoise = new SimplexNoiseField(
// 	TerrainCurve,
// 	wLength,
// 	wHeight,
// 	amplitude,
// 	frequency
// 	// Noise.SimplexField( new Noise.FractalParameters( Seed: r, Frequency: frequency ) )
// );
// Log.Info( simplexNoise );
// var terrainSdf = new NoiseSdf2D(
// 	new Vector2( -wLength / 2f, 0 ), 
// 	new Vector2( wLength / 2f, wHeight ), 
// 	simplexNoise );

// for ( var x = 0; x < pointsX; x++ )
// {
// 	for ( var y = 0; y < pointsY; y++ )
// 	{
// 		var xRel = x / (float)pointsX;
// 		var yRel = y / (float)pointsY;
// 		var fade = TerrainCurve.Evaluate( xRel );
// 		var noise = Noise.Simplex( xRel * freq + r, yRel * freq + r );
// 		var value = fade * pointsY / y * noise;
// 		
// 		data.SetPixel( pointsX, x, y, value > 0.5f ? Color.Black : Color.White );
// 	}
// }
//
// var cfg = new MaterialsConfig( true, true );
// var materials = GetActiveMaterials( cfg );
//
// var texSdf = new TextureSdf( tex, 10, pointsX );
// SdfWorld.AddAsync( texSdf, materials.ElementAt( 0 ).Key );
