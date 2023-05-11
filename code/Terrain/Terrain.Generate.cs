using Sandbox.Sdf;

namespace Grubs;

public partial class Terrain
{
	void SetupGeneratedWorld()
	{
		AddWorldBox( GrubsConfig.TerrainLength, GrubsConfig.TerrainHeight );

		GenerateWorld();
	}

	private bool[,] TerrainMap;
	private int[] NoiseMap;
	private float[,] DensityMap;

	private float amplitude = 48f;
	private float frequency = 3.2f;

	private float noiseMin = 0.45f;
	private float noiseMax = 0.55f;

	private float resolution = 8f;

	void GenerateWorld()
	{
		var wLength = GrubsConfig.TerrainLength;
		var wHeight = GrubsConfig.TerrainHeight;

		var pointsX = (wLength / resolution).CeilToInt();
		var pointsY = (wHeight / resolution).CeilToInt();

		TerrainMap = new bool[pointsX, pointsY];
		NoiseMap = new int[pointsX];
		DensityMap = new float[pointsX, pointsY];

		var r = Game.Random.Int( 99999 );

		for ( var x = 0; x < pointsX; x++ )
		{
			for ( var y = 0; y < pointsY; y++ )
			{
				TerrainMap[x, y] = false;
			}
		}

		// Generate Noise Map.
		int maxY = 0;
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

		var bb = new Vector2( 0, maxY * resolution );
		var aa = new Vector2( wLength, pointsY * resolution );
		SubtractBox( bb, aa );

		// Populate Density Map for unique terrain features.
		for ( var x = 0; x < pointsX; x++ )
		{
			for ( var y = 0; y < maxY; y++ )
			{
				DensityMap[x, y] = Noise.Simplex( x + r, y + r );

				if ( DensityMap[x, y] > noiseMin && DensityMap[x, y] < noiseMax )
				{
					TerrainMap[x, y] = false;
				}

				// Subtract from the world in passing.
				if ( !TerrainMap[x, y] )
				{
					var midpoint = new Vector2( x * resolution, y * resolution );
					// TODO: figure out best way to subtract
					SubtractCircle( midpoint, 8f );
					// SubtractBox( midpoint - 8f, midpoint + 8f, 4f );
				}
			}
		}
	}

	private float GetNoise( int x, int y )
	{
		return amplitude * Noise.Perlin( x * frequency, y * frequency );
	}
}
