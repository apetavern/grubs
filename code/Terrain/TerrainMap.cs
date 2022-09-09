using Grubs.Utils;

namespace Grubs.Terrain;

public partial class TerrainMap
{
	public bool[,] TerrainGrid { get; set; }

	public int Width => GameConfig.TerrainWidth;

	public int Height => GameConfig.TerrainHeight;

	public int Seed { get; set; } = 0;

	private readonly float surfaceLevel = 0.50f;
	private readonly float noiseThreshold = 0.25f;
	private readonly int borderWidth = 5;

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
				TerrainGrid[x, z] = Noise.Simplex( (x + Seed) * res, (z + Seed) * res ) > surfaceLevel;
	}

	private void AddBorder()
	{
		bool[,] borderedMap = new bool[Width + borderWidth * 2, Height + borderWidth * 2];
		for ( int x = 0; x < borderedMap.GetLength( 0 ); x++ )
			for ( int z = 0; z < borderedMap.GetLength( 1 ); z++ )
			{
				if ( x >= borderWidth && x < Width + borderWidth && z >= borderWidth && z < Height + borderWidth )
				{
					borderedMap[x, z] = TerrainGrid[x - borderWidth, z - borderWidth];
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
				TerrainGrid[x, z] = n > noiseThreshold;
			}
	}

	public int[,] regionGrid { get; set; }

	struct IntVector3
	{
		public int x { get; set; }
		public int y { get; set; }
		public int z { get; set; }

		public IntVector3( int x, int y, int z )
		{
			this.x = x;
			this.y = y;
			this.z = z;
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
		regionGrid = new int[Width, Height];

		for ( int x = 0; x < Width; x++ )
			for ( int z = 0; z < Height; z++ )
				regionGrid[x, z] = TerrainGrid[x, z] ? 1 : 0;

		for ( int x = 0; x < Width; x++ )
			for ( int z = 0; z < Height; z++ )
			{
				// Terrain exists in this location.
				if ( regionGrid[x, z] == 1 )
				{
					queue.Enqueue( new IntVector3( x, z, label ) );
					regionGrid[x, z] = label;

					while ( queue.Count > 0 )
					{
						var current = queue.Dequeue();
						if ( current.x - 1 >= 0 )
						{
							if ( regionGrid[current.x - 1, current.y] == 1 )
							{
								regionGrid[current.x - 1, current.y] = label;
								queue.Enqueue( new IntVector3( current.x - 1, current.y, label ) );
							}
						}

						if ( current.x + 1 < Width )
						{
							if ( regionGrid[current.x + 1, current.y] == 1 )
							{
								regionGrid[current.x + 1, current.y] = label;
								queue.Enqueue( new IntVector3( current.x + 1, current.y, label ) );
							}
						}

						if ( current.y - 1 >= 0 )
						{
							if ( regionGrid[current.x, current.y - 1] == 1 )
							{
								regionGrid[current.x, current.y - 1] = label;
								queue.Enqueue( new IntVector3( current.x, current.y - 1, label ) );
							}
						}

						if ( current.y + 1 < Height )
						{
							if ( regionGrid[current.x, current.y + 1] == 1 )
							{
								regionGrid[current.x, current.y + 1] = label;
								queue.Enqueue( new IntVector3( current.x, current.y + 1, label ) );
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
		int threshold = MathX.FloorToInt( Height * 0.4f );

		for ( int x = 0; x < Width; x++ )
			for ( int z = 0; z < threshold; z++ )
				if ( regionGrid[x, z] != 0 )
					regionIdsToKeep.Add( regionGrid[x, z] );

		for ( int x = 0; x < Width; x++ )
			for ( int z = 0; z < Height; z++ )
				if ( !regionIdsToKeep.Contains( regionGrid[x, z] ) )
					TerrainGrid[x, z] = false;
	}

	public List<Vector2> PositionsToDilate;

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
	public void DestructSphere( Vector2 midpoint, float size )
	{
		var scale = GameConfig.TerrainScale;

		for ( int i = 0; i < TerrainGrid.GetLength( 0 ); i++ )
		{
			for ( int j = 0; j < TerrainGrid.GetLength( 1 ); j++ )
			{
				var xDiff = midpoint.x - (i * scale);
				var yDiff = midpoint.y - (j * scale);
				var d = Math.Sqrt( Math.Pow( xDiff, 2 ) + Math.Pow( yDiff, 2 ) );

				if ( d < size )
					TerrainGrid[i, j] = false;
			}
		}
	}

	/// <summary>
	/// Destruct a sphere in the terrain grid.
	/// </summary>
	/// <param name="Startpoint">The Vector3 startpoint of the line to be destructed.</param>
	/// <param name="EndPoint">The Vector3 endpoint of the line to be destructed.</param>
	/// <param name="Width">The size (radius) of the sphere to be destructed.</param>
	public void DestructLine( Vector3 Startpoint, Vector3 EndPoint, float Width )
	{
		Vector3 TotalLength = (Startpoint - EndPoint);

		int StepCount = (int)MathF.Round( TotalLength.Length / (Width) );

		Vector3 CurrentPoint = Startpoint;

		for ( int i = 0; i < StepCount; i++ )
		{
			CurrentPoint = Vector3.Lerp( Startpoint, EndPoint, (float)i / StepCount );

			var pos = new Vector3( CurrentPoint.x, CurrentPoint.z );

			DestructSphere( pos, Width );
		}
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
