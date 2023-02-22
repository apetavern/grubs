namespace Grubs;

public partial class World
{
	private void SetupGenerateWorld()
	{
		GenerateAlt();
		// GenerateRandomWorld();
	}

	/*
	 * Terrain Map: a 2D array that explains whether each position should be on or off.
	 * Noise Map: The baseline noise map to determine the terrain height at each point.
	 * Density Map: a 2D array that determines whether a position in the terrain map can be removed.
	 */

	private bool[,] TerrainMap;
	private int[] NoiseMap;
	private float[,] DensityMap;

	private float amplitude = 48f;
	private float frequency = 3f;

	private float noiseMin = 0.45f;
	private float noiseMax = 0.55f;

	private void GenerateAlt()
	{
		CsgWorld.Add( CubeBrush, SandMaterial, scale: new Vector3( WorldLength, WorldWidth, WorldHeight ), position: new Vector3( 0, 0, -WorldHeight / 2 ) );
		CsgBackground.Add( CubeBrush, RockMaterial, scale: new Vector3( WorldLength, WorldWidth, WorldHeight ), position: new Vector3( 0, 64, -WorldHeight / 2 ) );

		var pointsX = (WorldLength / _resolution).CeilToInt();
		var pointsZ = (WorldHeight / _resolution).CeilToInt();

		TerrainMap = new bool[pointsX, pointsZ];
		NoiseMap = new int[pointsX];
		DensityMap = new float[pointsX, pointsZ];

		var r = Game.Random.Int( 99999 );

		for ( var x = 0; x < pointsX; x++ )
		{
			for ( var z = 0; z < pointsZ; z++ )
			{
				TerrainMap[x, z] = false;
			}
		}

		// Generate Noise Map.
		for ( var x = 0; x < pointsX; x++ )
		{
			NoiseMap[x] = GetNoise( x + r, 0 ).FloorToInt();
			if ( NoiseMap[x] >= pointsZ )
				NoiseMap[x] = pointsZ - 1;
			TerrainMap[x, NoiseMap[x]] = true;

			for ( var z = NoiseMap[x]; z >= 0; z-- )
			{
				TerrainMap[x, z] = true;
			}
		}

		// Do the background.
		for ( var x = 0; x < pointsX; x++ )
		{
			for ( var z = 0; z < pointsZ; z++ )
			{
				if ( !TerrainMap[x, z] )
				{
					var paddedRes = _resolution + (_resolution * 0.75f);
					var min = new Vector3( x * _resolution - paddedRes, -WorldWidth / 2 + 64, z * _resolution - paddedRes );
					var max = new Vector3( x * _resolution + paddedRes, WorldWidth / 2 + 64, z * _resolution + paddedRes );

					min -= new Vector3( WorldLength / 2, 0, WorldHeight );
					max -= new Vector3( WorldLength / 2, 0, WorldHeight );
					SubtractBackground( min, max );
				}
			}
		}

		// Populate Density maps for caves and update TerrainMap from it.
		for ( var x = 0; x < pointsX; x++ )
		{
			for ( var z = 0; z < pointsZ; z++ )
			{
				DensityMap[x, z] = Noise.Simplex( x, z );

				if ( DensityMap[x, z] > noiseMin && DensityMap[x, z] < noiseMax )
				{
					TerrainMap[x, z] = false;
				}
			}
		}

		// Subtract from the main world according to the terrain grid.
		// We can speed this up by implementing a backtracking algorithm.
		// We should aim to use as few subtractions as possible.
		for ( var x = 0; x < pointsX; x++ )
		{
			for ( var z = 0; z < pointsZ; z++ )
			{
				if ( !TerrainMap[x, z] )
				{
					var paddedRes = _resolution + (_resolution * 0.75f);
					var min = new Vector3( x * _resolution - paddedRes, -WorldWidth / 2, z * _resolution - paddedRes );
					var max = new Vector3( x * _resolution + paddedRes, WorldWidth / 2, z * _resolution + paddedRes );

					min -= new Vector3( WorldLength / 2, 0, WorldHeight );
					max -= new Vector3( WorldLength / 2, 0, WorldHeight );
					SubtractDefault( min, max );

					var avg = (min + max) / 2;
					PossibleSpawnPoints.Add( avg );
				}
			}
		}
	}

	private float GetNoise( int x, int y )
	{
		return amplitude * Noise.Perlin( x * frequency, y * frequency );
	}

	private void GenerateRandomWorld()
	{
		CsgWorld.Add( CubeBrush, SandMaterial, scale: new Vector3( WorldLength, WorldWidth, WorldHeight ), position: new Vector3( 0, 0, -WorldHeight / 2 ) );
		CsgBackground.Add( CubeBrush, RockMaterial, scale: new Vector3( WorldLength, WorldWidth, WorldHeight ), position: new Vector3( 0, 64, -WorldHeight / 2 ) );

		PossibleSpawnPoints.Clear();
		var pointsX = (WorldLength / _resolution).CeilToInt();
		var pointsZ = (WorldHeight / _resolution).CeilToInt();

		_terrainGrid = new float[pointsX, pointsZ];

		var r = Game.Random.Int( 99999 );

		// Initialize Perlin noise grid.
		for ( var x = 0; x < pointsX; x++ )
		{
			for ( var z = 0; z < pointsZ; z++ )
			{
				var n = Noise.Perlin( (x + r) * _zoom, r, (z + r) * _zoom );
				n = Math.Abs( (n * 2) - 1 );
				_terrainGrid[x, z] = n;

				// Subtract from the solid where the noise is under a certain threshold.
				if ( _terrainGrid[x, z] < 0.15f )
				{
					// Pad the subtraction so the subtraction is more clean.
					var paddedRes = _resolution + (_resolution * 0.75f);

					var min = new Vector3( (x * _resolution) - paddedRes, -32, (z * _resolution) - paddedRes );
					var max = new Vector3( (x * _resolution) + paddedRes, 32, (z * _resolution) + paddedRes );

					// Offset by position.
					min -= new Vector3( WorldLength / 2, 0, WorldHeight );
					max -= new Vector3( WorldLength / 2, 0, WorldHeight );
					SubtractDefault( min, max );

					var avg = (min + max) / 2;
					PossibleSpawnPoints.Add( avg );
				}
			}
		}
	}
}
