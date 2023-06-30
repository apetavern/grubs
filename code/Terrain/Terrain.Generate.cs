namespace Grubs;

public partial class Terrain
{
	void SetupGeneratedWorld()
	{
		var cfg = new MaterialsConfig( true, true );
		var materials = GetActiveMaterials( cfg );
		AddWorldBox(
			GrubsConfig.TerrainLength,
			GrubsConfig.TerrainHeight,
			materials.ElementAt( 0 ).Key,
			materials.ElementAt( 1 ).Key );

		WorldTextureLength = GrubsConfig.TerrainLength;
		WorldTextureHeight = GrubsConfig.TerrainHeight;

		GenerateWorld();
	}

	private bool[,] TerrainMap;
	private int[] NoiseMap;
	private float[,] DensityMap;
	private bool[,] BackgroundMap;

	private float amplitude = 36f;
	private float frequency = 3.2f;

	private float noiseMin = 0.45f;
	private float noiseMax = 0.55f;
	private float noiseZoom = 2f;

	private float resolution = 8f;

	private int maxY;

	void GenerateWorld()
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
			NoiseMap[x] = (GetNoise( x + r, 0 ) * wHeight / 1024f).FloorToInt();
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

		toSubtract.Sort( ( a, b ) => (a.GetHashCode() & 0xff) - (b.GetHashCode() & 0xff) );

		foreach ( var midpoint in toSubtract )
		{
			// TODO: figure out best way to subtract
			SubtractCircle( midpoint, 8f, fgMaterials, worldOffset: true );
		}

		SubtractBackground( pointsX );
	}

	void SubtractBackgroundBox( int wLength, int pointsY )
	{
		var bb = new Vector2( 0, maxY * resolution );
		var aa = new Vector2( wLength, pointsY * resolution );
		SubtractBox( bb, aa, GetActiveMaterials( new MaterialsConfig( includeBackground: true ) ), worldOffset: true );
	}

	void SubtractBackground( int pointsX )
	{
		var materialsConfig = new MaterialsConfig( includeForeground: false, includeBackground: true );
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

		toSubtract.Sort( ( a, b ) => Random.Shared.Int( 2 ) * 2 - 1 );

		foreach ( var midpoint in toSubtract )
		{
			SubtractCircle( midpoint, 8f, bgMaterials, worldOffset: true );
		}
	}

	void SubtractForeground( int pointsX )
	{
		var fgMaterials = GetActiveMaterials( MaterialsConfig.Default );
		for ( var x = 0; x < pointsX; x++ )
		{
			for ( var y = 0; y < maxY; y++ )
			{
				if ( !TerrainMap[x, y] )
				{
					var midpoint = new Vector2( x * resolution, y * resolution );
					SubtractCircle( midpoint, 8f, fgMaterials, worldOffset: true );
				}
			}
		}
	}

	private float GetNoise( int x, int y )
	{
		return amplitude * Noise.Perlin( x * frequency, y * frequency );
	}
}
