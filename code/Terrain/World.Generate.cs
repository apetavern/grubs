namespace Grubs;

public partial class World
{
	private void SetupGenerateWorld()
	{
		CsgWorld.Add( 
			CubeBrush, 
			SandMaterial, 
			scale: new Vector3( WorldLength, WorldWidth, WorldHeight ), 
			position: new Vector3( 0, 0, WorldHeight / 2 ) );
		CsgBackground.Add( 
			CubeBrush, RockMaterial, 
			scale: new Vector3( WorldLength, WorldWidth, WorldHeight ),
			position: new Vector3( 0, 64, WorldHeight / 2 ) );
		GenerateAlt();
		SetupKillZone( WorldHeight );
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
	private float frequency = 3.2f;

	private float noiseMin = 0.45f;
	private float noiseMax = 0.55f;

	private void GenerateAlt()
	{
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

		int maxZ = 0;
		// Generate Noise Map.
		for ( var x = 0; x < pointsX; x++ )
		{
			NoiseMap[x] = (GetNoise( x + r, 0 ) * GrubsConfig.TerrainHeight / 1024f).FloorToInt();
			if ( NoiseMap[x] >= pointsZ )
				NoiseMap[x] = pointsZ - 1;
			TerrainMap[x, NoiseMap[x]] = true;

			for ( var z = NoiseMap[x]; z >= 0; z-- )
			{
				maxZ = Math.Max( maxZ, z );
				TerrainMap[x, z] = true;
			}
		}

		// Do the background.
		for ( var x = 0; x < pointsX; x++ )
		{
			for ( var z = 0; z < maxZ + 1; z++ )
			{
				if ( !TerrainMap[x, z] )
				{
					var paddedRes = _resolution + (_resolution * 0.75f);
					var min = new Vector3( x * _resolution - paddedRes, -WorldWidth / 2 + 64, z * _resolution - paddedRes );
					var max = new Vector3( x * _resolution + paddedRes, WorldWidth / 2 + 64, z * _resolution + paddedRes );

					min -= new Vector3( WorldLength / 2, 0, 0 );
					max -= new Vector3( WorldLength / 2, 0, 0 );
					SubtractBackground( min, max );
				}
			}
		}


		var bb = new Vector3( 0, -WorldWidth / 2 + 64, pointsZ * _resolution );
		var aa = new Vector3( pointsX * _resolution, WorldWidth / 2 + 64, maxZ * _resolution );

		aa -= new Vector3( WorldLength / 2, 0, 0 );
		bb -= new Vector3( WorldLength / 2, 0, 0 );
		SubtractBackground( CubeBrush, aa, bb );

		aa = aa.WithY( aa.y - 64 );
		bb = bb.WithY( bb.y - 64 );
		SubtractDefault( CubeBrush, aa, bb );

		// Populate Density maps for caves and update TerrainMap from it.
		for ( var x = 0; x < pointsX; x++ )
		{
			for ( var z = 0; z < pointsZ; z++ )
			{
				DensityMap[x, z] = Noise.Simplex( x + r, z + r );

				if ( DensityMap[x, z] > noiseMin && DensityMap[x, z] < noiseMax )
				{
					TerrainMap[x, z] = false;
				}
			}
		}

		for ( var x = 0; x < pointsX; x++ )
		{
			for ( var z = 0; z < maxZ + 1; z++ )
			{
				if ( !TerrainMap[x, z] )
				{
					var paddedRes = _resolution + (_resolution * 0.75f);
					var min = new Vector3( x * _resolution - paddedRes, -WorldWidth / 2, z * _resolution - paddedRes );
					var max = new Vector3( x * _resolution + paddedRes, WorldWidth / 2, z * _resolution + paddedRes );

					min -= new Vector3( WorldLength / 2, 0, 0 );
					max -= new Vector3( WorldLength / 2, 0, 0 );
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
}
