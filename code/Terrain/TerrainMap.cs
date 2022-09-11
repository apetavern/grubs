using Grubs.Utils;

namespace Grubs.Terrain;

public class TerrainMap
{
	public bool[,] TerrainGrid { get; private set; } = null!;
	private int[,] RegionGrid { get; set; } = null!;
	private List<Vector2> PositionsToDilate { get; set; } = null!;

	public static int Width => GameConfig.TerrainWidth;

	public static int Height => GameConfig.TerrainHeight;

	public int Seed { get; set; } = 0;

	private const float SurfaceLevel = 0.50f;
	private const float NoiseThreshold = 0.25f;
	private const int BorderWidth = 5;

	public TerrainMap()
	{
		Rand.SetSeed( (int)Time.Now );
		Seed = Rand.Int( 100000 );

		GenerateTerrainGrid();
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

	/// <summary>
	/// Generate a default terrain grid, using Simplex Noise above a certain surface level.
	/// </summary>
	private void DefaultGrid()
	{
		var res = GameConfig.TerrainResolution;

		for ( int x = 0; x < Width; x++ )
			for ( int z = 0; z < Height; z++ )
				TerrainGrid[x, z] = Noise.Simplex( (x + Seed) * res, (z + Seed) * res ) > SurfaceLevel;
	}

	private void AddBorder()
	{
		bool[,] borderedMap = new bool[Width + BorderWidth * 2, Height + BorderWidth * 2];
		for ( int x = 0; x < borderedMap.GetLength( 0 ); x++ )
			for ( int z = 0; z < borderedMap.GetLength( 1 ); z++ )
			{
				if ( x >= BorderWidth && x < Width + BorderWidth && z >= BorderWidth && z < Height + BorderWidth )
				{
					borderedMap[x, z] = TerrainGrid[x - BorderWidth, z - BorderWidth];
				}
				else
				{
					borderedMap[x, z] = true;
				}
			}

		TerrainGrid = borderedMap;
	}

	private void AlteredGrid()
	{
		GenerateTurbulentNoise();
		FindRegions();
		DiscardRegions();
		for ( int i = 0; i < GameConfig.DilationAmount; i++ )
			DilateRegions();
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

		for ( int x = 0; x < Width; x++ )
			for ( int z = 0; z < Height; z++ )
			{
				var n = Noise.Simplex( (x + Seed) * res, (z + Seed) * res );
				n = Math.Abs( (n * 2) - 1 );
				TerrainGrid[x, z] = n > NoiseThreshold;
			}
	}

	struct IntVector3
	{
		public int X { get; set; }
		public int Y { get; set; }
		public int Z { get; set; }

		public IntVector3( int x, int y, int z )
		{
			this.X = x;
			this.Y = y;
			this.Z = z;
		}
	}

	/// <summary>
	/// A region extraction algorithm to determine each unique region of the terrain.
	/// See: https://en.wikipedia.org/wiki/Connected-component_labeling
	/// </summary>
	private void FindRegions()
	{
		int label = 2;
		var queue = new Queue<IntVector3>();
		RegionGrid = new int[Width, Height];

		for ( int x = 0; x < Width; x++ )
			for ( int z = 0; z < Height; z++ )
				RegionGrid[x, z] = TerrainGrid[x, z] ? 1 : 0;

		for ( int x = 0; x < Width; x++ )
			for ( int z = 0; z < Height; z++ )
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
		int threshold = (Height * 0.4f).FloorToInt();

		for ( int x = 0; x < Width; x++ )
			for ( int z = 0; z < threshold; z++ )
				if ( RegionGrid[x, z] != 0 )
					regionIdsToKeep.Add( RegionGrid[x, z] );

		for ( int x = 0; x < Width; x++ )
			for ( int z = 0; z < Height; z++ )
				if ( !regionIdsToKeep.Contains( RegionGrid[x, z] ) )
					TerrainGrid[x, z] = false;
	}

	/// <summary>
	/// Morphologically dilate the regions to reduce the amount of space in between them.
	/// </summary>
	private void DilateRegions()
	{
		PositionsToDilate = new List<Vector2>();

		for ( int x = 0; x < Width; x++ )
			for ( int z = 0; z < Height; z++ )
				if ( TerrainGrid[x, z] )
				{
					CheckForDilationPosition( x, z );
				}

		foreach ( var position in PositionsToDilate )
		{
			TerrainGrid[(int)position.x, (int)position.y] = true;
		}
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
	public bool DestructSphere( Vector2 midpoint, float size )
	{
		var scale = GameConfig.TerrainScale;
		bool modifiedTerrain = false;
		for ( int i = 0; i < TerrainGrid.GetLength( 0 ); i++ )
		{
			for ( int j = 0; j < TerrainGrid.GetLength( 1 ); j++ )
			{
				var xDiff = midpoint.x - (i * scale);
				var yDiff = midpoint.y - (j * scale);
				var d = Math.Sqrt( Math.Pow( xDiff, 2 ) + Math.Pow( yDiff, 2 ) );

				if ( d < size )
				{
					TerrainGrid[i, j] = false;
					modifiedTerrain = true;
				}
			}
		}

		return modifiedTerrain;
	}

	/// <summary>
	/// Destruct a sphere in the terrain grid.
	/// </summary>
	/// <param name="startPoint">The Vector3 startpoint of the line to be destructed.</param>
	/// <param name="endPoint">The Vector3 endpoint of the line to be destructed.</param>
	/// <param name="width">The size (radius) of the sphere to be destructed.</param>
	public bool DestructLine( Vector3 startPoint, Vector3 endPoint, float width )
	{
		Vector3 totalLength = (startPoint - endPoint);

		int stepCount = (int)MathF.Round( totalLength.Length / (width) );

		Vector3 currentPoint = startPoint;

		bool modifiedTerrain = false;

		for ( int i = 0; i < stepCount; i++ )
		{
			currentPoint = Vector3.Lerp( startPoint, endPoint, (float)i / stepCount );

			var pos = new Vector3( currentPoint.x, currentPoint.z );

			modifiedTerrain = DestructSphere( pos, width );
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
			int x = Rand.Int( Width - 1 );
			int z = Rand.Int( Height - 1 );
			if ( !TerrainGrid[x, z] )
			{
				var startPos = new Vector3( x * scale, 0, z * scale );
				var tr = Trace.Ray( startPos, startPos + Vector3.Down * Height * scale ).WithTag( "solid" ).Run();
				if ( tr.Hit )
					return tr.EndPosition;
			}
		}
	}
}
