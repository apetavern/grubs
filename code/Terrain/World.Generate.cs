namespace Grubs;

public partial class World
{
	private void SetupGenerateWorld()
	{
		GenerateAlt();
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

					min -= new Vector3( WorldLength / 2, 0, WorldHeight );
					max -= new Vector3( WorldLength / 2, 0, WorldHeight );
					SubtractBackground( min, max );
				}
			}
		}
		

		var bb = new Vector3( 0, -WorldWidth / 2 + 64, pointsZ * _resolution );
		var aa = new Vector3( pointsX * _resolution, WorldWidth /2 + 64, maxZ * _resolution );

		aa -= new Vector3( WorldLength / 2, 0, WorldHeight );
		bb -= new Vector3( WorldLength / 2, 0, WorldHeight );
		SubtractBackground( CubeBrush, aa, bb );

		aa = aa.WithY( aa.y - 64 );
		bb = bb.WithY( bb.y - 64 );
		SubtractDefault(CubeBrush, aa, bb );

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

/*	private bool AllFalse( bool[,] grid, int top, int bottom, int left, int right )
	{
		for ( int i = top; i < bottom; i++ )
		{
			for ( int j = left; j < right; j++ )
			{
				if ( grid[i, j] )
				{
					return false;
				}
			}
		}

		return true;
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
	}*/

	/*var visited = new bool[pointsX, pointsZ];

		for ( var x = 0; x < pointsX; x++ )
		{
			for ( var z = 0; z < pointsZ; z++ )
			{
				if ( TerrainMap[x, z] )
				{
					if ( x - 1 > 0 && x + 1 < pointsX && z - 1 > 0 && z + 1 < pointsZ )
					{
						if ( !TerrainMap[x - 1, z] )
						{
							SubtractAtPoint( x - 1, z );
							visited[x - 1, z] = true;
						}

						if ( !TerrainMap[x, z - 1] )
						{
							SubtractAtPoint( x, z - 1 );
							visited[x, z - 1] = true;
						}

						if ( !TerrainMap[x + 1, z] )
						{
							SubtractAtPoint( x + 1, z );
							visited[x + 1, z] = true;
						}

						if ( !TerrainMap[x, z + 1] )
						{
							SubtractAtPoint( x, z + 1 );
							visited[x, z + 1] = true;
						}
					}
				}
			}
		}*/

	/*for ( var x = 0; x < pointsX; x++ )
		{
			for ( var z = 0; z < pointsZ; z++ )
			{
				if ( TerrainMap[x, z] )
				{
					if ( x - 1 > 0 && x + 1 < pointsX && z - 1 > 0 && z + 1 < pointsZ )
					{
						if ( !TerrainMap[x - 1, z] )
						{
							SubtractAtPointFg( x - 1, z );
						}

						if ( !TerrainMap[x, z - 1] )
						{
							SubtractAtPointFg( x, z - 1 );
						}

						if ( !TerrainMap[x + 1, z] )
						{
							SubtractAtPointFg( x + 1, z );
						}

						if ( !TerrainMap[x, z + 1] )
						{
							SubtractAtPointFg( x, z + 1 );
						}
					}
				}
			}
		}*/

	/*rects = new List<Vector2>();
		top = 0;
		left = 0;
		while ( top < pointsX )
		{
			while ( left < pointsZ )
			{
				if ( TerrainMap[top, left] )
				{
					left++;
					continue;
				}

				var right = left;
				while ( right < pointsZ && !TerrainMap[top, right] )
				{
					right += 1;
				}
				var bottom = top;
				while ( bottom < pointsX && AllFalse( TerrainMap, top, bottom, left, right ) )
				{
					bottom++;
				}

				rects.Add( new Vector2( top, left ) );
				rects.Add( new Vector2( bottom - 1, right - 1 ) );

				left = right;
			}

			left = 0;
			top++;
		}

		for ( int i = 0; i < rects.Count; i += 2 )
		{
			var xMin = rects[i].x;
			var zMin = rects[i].y;

			var xMax = rects[i + 1].x;
			var zMax = rects[i + 1].y;

			var paddedRes = _resolution + (_resolution * 0.75f);

			var min = new Vector3( xMin * _resolution - paddedRes, -WorldWidth / 2, zMin * _resolution - paddedRes );
			var max = new Vector3( xMax * _resolution + paddedRes, WorldWidth / 2, zMax * _resolution + paddedRes );

			min -= new Vector3( WorldLength / 2, 0, WorldHeight );
			max -= new Vector3( WorldLength / 2, 0, WorldHeight );

			DebugOverlay.Box( min, max, Color.Random, 30f );
			SubtractDefault( CoolBrush, min, max );
		}*/

	// Subtract from the background.
	/*var rects = new List<Vector2>();
	var top = 0;
	var left = 0;
	while ( top < pointsX )
	{
		while ( left < pointsZ )
		{
			if ( TerrainMap[top, left] )
			{
				left++;
				continue;
			}

			var right = left;
			while ( right < pointsZ && !TerrainMap[top, right] )
			{
				right += 1;
			}
			var bottom = top;
			while ( bottom < pointsX && AllFalse( TerrainMap, top, bottom, left, right ) )
			{
				bottom++;
			}

			rects.Add( new Vector2( top, left ) );
			rects.Add( new Vector2( bottom, right ) );

			left = right;
		}

		left = 0;
		top++;
	}

	for ( int i = 0; i < rects.Count; i += 2 )
	{
		var xMin = rects[i].x;
		var zMin = rects[i].y;

		var xMax = rects[i + 1].x;
		var zMax = rects[i + 1].y;

		var min = new Vector3( xMin * _resolution, -WorldWidth / 2 + 64, zMin * _resolution );
		var max = new Vector3( xMax * _resolution, WorldWidth / 2 + 64, zMax * _resolution);

		min -= new Vector3( WorldLength / 2, 0, WorldHeight );
		max -= new Vector3( WorldLength / 2, 0, WorldHeight );

		DebugOverlay.Box( min, max, Color.Random, 30f );
		SubtractBackground( CoolBrush, min, max );
	}*/

	/*	private void SubtractAtPoint( int x, int z )
	{
		var paddedRes = _resolution + (_resolution * 0.75f);
		var min = new Vector3( x * _resolution - paddedRes, -WorldWidth / 2 + 64, z * _resolution - paddedRes );
		var max = new Vector3( x * _resolution + paddedRes, WorldWidth / 2 + 64, z * _resolution + paddedRes );

		min -= new Vector3( WorldLength / 2, 0, WorldHeight );
		max -= new Vector3( WorldLength / 2, 0, WorldHeight );
		SubtractBackground( min, max );
	}

	private void SubtractAtPointFg( int x, int z )
	{
		var paddedRes = _resolution + (_resolution * 0.75f);

		var min = new Vector3( x * _resolution - paddedRes, -WorldWidth / 2, z * _resolution - paddedRes );
		var max = new Vector3( x * _resolution + paddedRes, WorldWidth / 2, z * _resolution + paddedRes );

		min -= new Vector3( WorldLength / 2, 0, WorldHeight );
		max -= new Vector3( WorldLength / 2, 0, WorldHeight );
		SubtractDefault( min, max );
	}*/
}
