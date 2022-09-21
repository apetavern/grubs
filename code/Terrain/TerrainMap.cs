using Grubs.Utils;

namespace Grubs.Terrain;

public sealed class TerrainMap
{
	public bool[,] TerrainGrid { get; private set; } = null!;
	public List<TerrainChunk> TerrainGridChunks { get; private set; } = null!;
	private int[,] RegionGrid { get; set; } = null!;
	private List<Vector2> PositionsToDilate { get; set; } = null!;

	public static int Width => GameConfig.TerrainWidth;

	public static int Height => GameConfig.TerrainHeight;

	public int Seed { get; set; }

	private const float SurfaceLevel = 0.50f;
	private const float NoiseThreshold = 0.25f;
	private const int BorderWidth = 5;
	private const int ChunkSize = 10;

	public TerrainMap( int seed )
	{
		Seed = seed;

		GenerateTerrainGrid();
		AssignGridToChunks();
	}

	/// <summary>
	/// Generate a terrain grid based on various game configuration options.
	/// </summary>
	public void GenerateTerrainGrid()
	{
		TerrainGrid = new bool[Width, Height];

		if ( GameConfig.AlteredTerrain )
			AlteredGrid();
		else
			DefaultGrid();

		if ( GameConfig.TerrainBorder )
			AddBorder();
	}

	private void AssignGridToChunks()
	{
		TerrainGridChunks = new List<TerrainChunk>();
		var chunkCount = (Width * Height) / (ChunkSize * ChunkSize);

		var xOffset = 0;
		var yOffset = 0;
		for ( var i = 0; i < chunkCount; i++ )
		{
			var scale = GameConfig.TerrainScale;
			var chunkPos = new Vector3( xOffset * scale, 0, yOffset * scale );
			var chunk = new TerrainChunk( chunkPos )
			{
				TerrainGrid = new bool[ChunkSize, ChunkSize]
			};

			var chunksArr = TerrainGridChunks.ToArray();

			// Set chunk neighbours for the purpose of connecting chunks.
			if ( xOffset > 0 )
			{
				chunksArr[i - 1].xNeighbour = chunk;
			}

			if ( yOffset > 0 )
			{
				chunksArr[i - (Width / ChunkSize)].yNeighbour = chunk;
				if ( xOffset > 0 )
				{
					chunksArr[i - (Width / ChunkSize) - 1].xyNeighbour = chunk;
				}
			}

			for ( var x = xOffset; x < xOffset + ChunkSize; x++ )
			{
				for ( var y = yOffset; y < yOffset + ChunkSize; y++ )
				{
					chunk.TerrainGrid[x % ChunkSize, y % ChunkSize] = TerrainGrid[x, y];
				}
			}

			TerrainGridChunks.Add( chunk );

			xOffset += ChunkSize;
			if ( xOffset == Width )
			{
				xOffset = 0;
				yOffset += ChunkSize;
			}
		}
	}

	private void AlteredGrid()
	{
		GenerateTurbulentNoise();
		FindRegions();
		DiscardRegions();
		for ( var i = 0; i < GameConfig.DilationAmount; i++ )
			DilateRegions();
	}

	/// <summary>
	/// Generate a default terrain grid, using Simplex Noise above a certain surface level.
	/// </summary>
	private void DefaultGrid()
	{
		var res = GameConfig.TerrainResolution;

		for ( var x = 0; x < Width; x++ )
			for ( var z = 0; z < Height; z++ )
				TerrainGrid[x, z] = Noise.Simplex( (x + Seed) * res, (z + Seed) * res ) > SurfaceLevel;
	}

	private void AddBorder()
	{
		var borderedMap = new bool[Width + BorderWidth * 2, Height + BorderWidth * 2];
		for ( var x = 0; x < borderedMap.GetLength( 0 ); x++ )
			for ( var z = 0; z < borderedMap.GetLength( 1 ); z++ )
			{
				if ( x >= BorderWidth && x < Width + BorderWidth && z >= BorderWidth && z < Height + BorderWidth )
					borderedMap[x, z] = TerrainGrid[x - BorderWidth, z - BorderWidth];
				else
					borderedMap[x, z] = true;
			}

		TerrainGrid = borderedMap;
	}

	/// <summary>
	/// Generates a turbulent variation of Simplex noise.
	/// Standard turbulent noise is the absolute value of [-1, 1] noise,
	/// but since Noise.Simplex returns floats between [0, 1], we multiply it by 2,
	/// subtract 1, and then take the absolute value. Then, we apply a threshold
	/// to get our "blobby" terrain.
	/// </summary>
	private void GenerateTurbulentNoise()
	{
		var res = GameConfig.TerrainResolution;

		for ( var x = 0; x < Width; x++ )
			for ( var z = 0; z < Height; z++ )
			{
				var n = Noise.Simplex( (x + Seed) * res, (z + Seed) * res );
				n = Math.Abs( (n * 2) - 1 );
				TerrainGrid[x, z] = n > NoiseThreshold;
			}
	}

	/// <summary>
	/// A region extraction algorithm to determine each unique region of the terrain.
	/// See: https://en.wikipedia.org/wiki/Connected-component_labeling
	/// </summary>
	private void FindRegions()
	{
		var label = 2;
		var queue = new Queue<IntVector3>();
		RegionGrid = new int[Width, Height];

		for ( var x = 0; x < Width; x++ )
			for ( var z = 0; z < Height; z++ )
				RegionGrid[x, z] = TerrainGrid[x, z] ? 1 : 0;

		for ( var x = 0; x < Width; x++ )
			for ( var z = 0; z < Height; z++ )
			{
				// Terrain exists in this location.
				if ( RegionGrid[x, z] == 1 )
				{
					queue.Enqueue( new IntVector3( x, z, label ) );
					RegionGrid[x, z] = label;

					while ( queue.Count > 0 )
					{
						var current = queue.Dequeue();
						if ( current.X - 1 >= 0 )
						{
							if ( RegionGrid[current.X - 1, current.Y] == 1 )
							{
								RegionGrid[current.X - 1, current.Y] = label;
								queue.Enqueue( new IntVector3( current.X - 1, current.Y, label ) );
							}
						}

						if ( current.X + 1 < Width )
						{
							if ( RegionGrid[current.X + 1, current.Y] == 1 )
							{
								RegionGrid[current.X + 1, current.Y] = label;
								queue.Enqueue( new IntVector3( current.X + 1, current.Y, label ) );
							}
						}

						if ( current.Y - 1 >= 0 )
						{
							if ( RegionGrid[current.X, current.Y - 1] == 1 )
							{
								RegionGrid[current.X, current.Y - 1] = label;
								queue.Enqueue( new IntVector3( current.X, current.Y - 1, label ) );
							}
						}

						if ( current.Y + 1 < Height )
						{
							if ( RegionGrid[current.X, current.Y + 1] == 1 )
							{
								RegionGrid[current.X, current.Y + 1] = label;
								queue.Enqueue( new IntVector3( current.X, current.Y + 1, label ) );
							}
						}
					}
				}

				label++;
			}
	}

	/// <summary>
	/// Discard regions that do not have members under the set threshold.
	/// </summary>
	private void DiscardRegions()
	{
		var regionIdsToKeep = new HashSet<int>();
		var threshold = (Height * 0.35f).FloorToInt();

		for ( var x = 0; x < Width; x++ )
			for ( var z = 0; z < threshold; z++ )
				if ( RegionGrid[x, z] != 0 )
					regionIdsToKeep.Add( RegionGrid[x, z] );

		for ( var x = 0; x < Width; x++ )
			for ( var z = 0; z < Height; z++ )
				if ( !regionIdsToKeep.Contains( RegionGrid[x, z] ) )
					TerrainGrid[x, z] = false;
	}

	/// <summary>
	/// Morphologically dilate the regions to reduce the amount of space in between them.
	/// </summary>
	private void DilateRegions()
	{
		PositionsToDilate = new List<Vector2>();

		for ( var x = 0; x < Width; x++ )
			for ( var z = 0; z < Height; z++ )
				if ( TerrainGrid[x, z] )
					CheckForDilationPosition( x, z );

		foreach ( var position in PositionsToDilate )
			TerrainGrid[(int)position.x, (int)position.y] = true;
	}

	/// <summary>
	/// Check neighbours for dilation.
	/// </summary>
	/// <param name="x">The x position we are dilating on.</param>
	/// <param name="z">The y position we are dilating on.</param>
	private void CheckForDilationPosition( int x, int z )
	{
		if ( x - 1 > 0 )
			PositionsToDilate.Add( new Vector2( x - 1, z ) );
		if ( z - 1 > 0 )
			PositionsToDilate.Add( new Vector2( x, z - 1 ) );
		if ( x - 1 > 0 && z - 1 > 0 )
			PositionsToDilate.Add( new Vector2( x - 1, z - 1 ) );
		if ( x + 1 < Width )
			PositionsToDilate.Add( new Vector2( x + 1, z ) );
		if ( z + 1 < Height )
			PositionsToDilate.Add( new Vector2( x, z + 1 ) );
		if ( x + 1 < Width && z + 1 < Height )
			PositionsToDilate.Add( new Vector2( x + 1, z + 1 ) );
	}

	/// <summary>
	/// Destruct a sphere in the terrain grid.
	/// </summary>
	/// <param name="midpoint">The Vector2 midpoint of the sphere to be destructed.</param>
	/// <param name="size">The size (radius) of the sphere to be destructed.</param>
	/// <returns>Whether or not the terrain has been modified.</returns>
	public bool DestructSphere( Vector2 midpoint, float size )
	{
		var scale = GameConfig.TerrainScale;
		var modifiedTerrain = false;

		for ( var i = 0; i < TerrainGrid.GetLength( 0 ); i++ )
		{
			for ( var j = 0; j < TerrainGrid.GetLength( 1 ); j++ )
			{
				var xDiff = midpoint.x - (i * scale);
				var yDiff = midpoint.y - (j * scale);
				var d = Math.Sqrt( Math.Pow( xDiff, 2 ) + Math.Pow( yDiff, 2 ) );

				if ( d >= size || !TerrainGrid[i, j] )
					continue;

				modifiedTerrain |= TogglePointInChunks( i, j );

			}
		}

		return modifiedTerrain;
	}

	private bool TogglePointInChunks( int x, int z )
	{
		var n = (x / ChunkSize) + (z / ChunkSize * (Width / ChunkSize));
		var xR = x % ChunkSize;
		var zR = z % ChunkSize;

		var chunk = TerrainGridChunks[n];
		if ( chunk.TerrainGrid[xR, zR] )
		{
			chunk.TerrainGrid[xR, zR] = false;
			chunk.IsDirty = true;
			return true;
		}

		return false;
	}

	/// <summary>
	/// Destruct a sphere in the terrain grid.
	/// </summary>
	/// <param name="startPoint">The Vector3 startpoint of the line to be destructed.</param>
	/// <param name="endPoint">The Vector3 endpoint of the line to be destructed.</param>
	/// <param name="width">The size (radius) of the sphere to be destructed.</param>
	/// <returns>Whether or not the terrain has been modified.</returns>
	public bool DestructLine( Vector3 startPoint, Vector3 endPoint, float width )
	{
		var totalLength = (startPoint - endPoint);
		var stepCount = (int)MathF.Round( totalLength.Length / width );
		var modifiedTerrain = false;

		for ( var i = 0; i < stepCount; i++ )
		{
			var currentPoint = Vector3.Lerp( startPoint, endPoint, (float)i / stepCount );
			var pos = new Vector3( currentPoint.x, currentPoint.z );
			modifiedTerrain |= DestructSphere( pos, width );
		}

		return modifiedTerrain;
	}

	/// <summary>
	/// Get a random spawn location using Sandbox.Rand.
	/// Traces down to the ground to ensure Grubs do not take damage when spawned.
	/// </summary>
	/// <returns>A Vector3 position a Grub can be spawned at.</returns>
	public Vector3 GetSpawnLocation()
	{
		var scale = GameConfig.TerrainScale;
		while ( true )
		{
			var x = Rand.Int( Width - 1 );
			var z = Rand.Int( Height - 1 );
			if ( TerrainGrid[x, z] )
				continue;

			var startPos = new Vector3( x * scale, 0, z * scale );
			var tr = Trace.Ray( startPos, startPos + Vector3.Down * Height * scale ).WithTag( "solid" ).Run();
			if ( tr.Hit )
				return tr.EndPosition;
		}
	}

	private readonly struct IntVector3
	{
		public readonly int X;
		public readonly int Y;
		public readonly int Z;

		public IntVector3( int x, int y, int z )
		{
			X = x;
			Y = y;
			Z = z;
		}
	}
}
