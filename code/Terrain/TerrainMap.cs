using Grubs.Utils;

namespace Grubs.Terrain;

[Category( "Terrain" )]
public sealed partial class TerrainMap : Entity
{
	[Net]
	public int Seed { get; private set; }

	[Net]
	public bool Premade { get; private set; }
	public readonly PremadeTerrain? PremadeMap;

	public bool[] TerrainGrid { get; private set; } = null!;
	public List<TerrainChunk> TerrainGridChunks { get; private set; } = null!;

	[Net]
	public int Width { get; private set; }
	[Net]
	public int Height { get; private set; }
	[Net]
	public new int Scale { get; private set; }
	[Net]
	public bool HasBorder { get; private set; }
	[Net]
	public TerrainType TerrainType { get; private set; }

	private readonly List<int> _pendingDestroyedIndexes = new();

	private const float SurfaceLevel = 0.50f;
	private const float NoiseThreshold = 0.25f;
	private const int BorderWidth = 5;
	private const int ChunkSize = 10;

	public TerrainMap()
	{
		Transmit = TransmitType.Always;
	}

	public TerrainMap( int seed ) : this()
	{
		Premade = false;
		PremadeMap = null;

		Width = GameConfig.TerrainWidth;
		Height = GameConfig.TerrainHeight;
		Scale = GameConfig.TerrainScale;
		HasBorder = GameConfig.TerrainBorder;
		TerrainType = GameConfig.TerrainType;

		Seed = seed;
		GenerateTerrainGrid();
		AssignGridToChunks();

		UpdateGridRpc( To.Everyone, TerrainGrid );
	}

	public TerrainMap( PremadeTerrain terrain ) : this()
	{
		Premade = true;
		PremadeMap = terrain;

		Width = terrain.Width;
		Height = terrain.Height;
		Scale = terrain.Scale;
		HasBorder = terrain.HasBorder;
		TerrainType = terrain.TerrainType;

		TerrainGrid = terrain.TerrainGrid;
		GenerateTerrainGrid();
		AssignGridToChunks();

		UpdateGridRpc( To.Everyone, TerrainGrid );
	}

	/// <summary>
	/// Generate a terrain grid based on various game configuration options.
	/// </summary>
	private void GenerateTerrainGrid()
	{
		if ( !Premade )
		{
			TerrainGrid = new bool[Width * Height];

			if ( GameConfig.AlteredTerrain )
				AlteredGrid();
			else
				DefaultGrid();
		}

		if ( HasBorder )
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
			var chunkPos = new Vector3( xOffset * Scale, 0, yOffset * Scale );
			var chunk = new TerrainChunk( chunkPos, this )
			{
				TerrainGrid = new bool[ChunkSize, ChunkSize]
			};

			// Set chunk neighbours for the purpose of connecting chunks.
			if ( xOffset > 0 )
			{
				TerrainGridChunks[i - 1].XNeighbour = chunk;
			}

			if ( yOffset > 0 )
			{
				TerrainGridChunks[i - (Width / ChunkSize)].YNeighbour = chunk;
				if ( xOffset > 0 )
				{
					TerrainGridChunks[i - (Width / ChunkSize) - 1].XyNeighbour = chunk;
				}
			}

			for ( var x = xOffset; x < xOffset + ChunkSize; x++ )
			{
				for ( var y = yOffset; y < yOffset + ChunkSize; y++ )
				{
					chunk.TerrainGrid[x % ChunkSize, y % ChunkSize] = TerrainGrid[Dimensions.Convert2dTo1d( x, y, Width )];
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
		var regionGrid = new int[Width, Height];

		GenerateTurbulentNoise();
		FindRegions( regionGrid );
		DiscardRegions( regionGrid );
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
			for ( var y = 0; y < Height; y++ )
				TerrainGrid[Dimensions.Convert2dTo1d( x, y, Width )] = Noise.Simplex( (x + Seed) * res, (y + Seed) * res ) > SurfaceLevel;
	}

	private void AddBorder()
	{
		var borderedMap = new bool[(Width + BorderWidth * 2) * (Height + BorderWidth * 2)];
		for ( var i = 0; i < borderedMap.Length; i++ )
		{
			var (x, y) = Dimensions.Convert1dTo2d( i, Width );
			if ( x >= BorderWidth && x < Width + BorderWidth && y >= BorderWidth && y < Height + BorderWidth )
				borderedMap[i] = TerrainGrid[Dimensions.Convert2dTo1d( x - BorderWidth, y - BorderWidth, Width )];
			else
				borderedMap[i] = true;
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
			for ( var y = 0; y < Height; y++ )
			{
				var n = Noise.Simplex( (x + Seed) * res, (y + Seed) * res );
				n = Math.Abs( (n * 2) - 1 );
				TerrainGrid[Dimensions.Convert2dTo1d( x, y, Width )] = n > NoiseThreshold;
			}
	}

	/// <summary>
	/// A region extraction algorithm to determine each unique region of the terrain.
	/// See: https://en.wikipedia.org/wiki/Connected-component_labeling
	/// </summary>
	private void FindRegions( int[,] regionGrid )
	{
		var label = 2;
		var queue = new Queue<IntVector2>();

		for ( var x = 0; x < Width; x++ )
			for ( var y = 0; y < Height; y++ )
				regionGrid[x, y] = TerrainGrid[Dimensions.Convert2dTo1d( x, y, Width )] ? 1 : 0;

		for ( var x = 0; x < Width; x++ )
			for ( var z = 0; z < Height; z++ )
			{
				// Terrain exists in this location.
				if ( regionGrid[x, z] == 1 )
				{
					queue.Enqueue( new IntVector2( x, z ) );
					regionGrid[x, z] = label;

					while ( queue.Count > 0 )
					{
						var current = queue.Dequeue();
						if ( current.X - 1 >= 0 )
						{
							if ( regionGrid[current.X - 1, current.Y] == 1 )
							{
								regionGrid[current.X - 1, current.Y] = label;
								queue.Enqueue( new IntVector2( current.X - 1, current.Y ) );
							}
						}

						if ( current.X + 1 < Width )
						{
							if ( regionGrid[current.X + 1, current.Y] == 1 )
							{
								regionGrid[current.X + 1, current.Y] = label;
								queue.Enqueue( new IntVector2( current.X + 1, current.Y ) );
							}
						}

						if ( current.Y - 1 >= 0 )
						{
							if ( regionGrid[current.X, current.Y - 1] == 1 )
							{
								regionGrid[current.X, current.Y - 1] = label;
								queue.Enqueue( new IntVector2( current.X, current.Y - 1 ) );
							}
						}

						if ( current.Y + 1 < Height )
						{
							if ( regionGrid[current.X, current.Y + 1] == 1 )
							{
								regionGrid[current.X, current.Y + 1] = label;
								queue.Enqueue( new IntVector2( current.X, current.Y + 1 ) );
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
	private void DiscardRegions( int[,] regionGrid )
	{
		var regionIdsToKeep = new HashSet<int>();
		var threshold = (Height * 0.35f).FloorToInt();

		for ( var x = 0; x < Width; x++ )
			for ( var z = 0; z < threshold; z++ )
				if ( regionGrid[x, z] != 0 )
					regionIdsToKeep.Add( regionGrid[x, z] );

		for ( var x = 0; x < Width; x++ )
			for ( var y = 0; y < Height; y++ )
				if ( !regionIdsToKeep.Contains( regionGrid[x, y] ) )
					TerrainGrid[Dimensions.Convert2dTo1d( x, y, Width )] = false;
	}

	/// <summary>
	/// Morphologically dilate the regions to reduce the amount of space in between them.
	/// </summary>
	private void DilateRegions()
	{
		var positionsToDilate = new List<IntVector2>();

		for ( var x = 0; x < Width; x++ )
			for ( var y = 0; y < Height; y++ )
				if ( TerrainGrid[Dimensions.Convert2dTo1d( x, y, Width )] )
					CheckForDilationPosition( positionsToDilate, x, y );

		foreach ( var position in positionsToDilate )
			TerrainGrid[Dimensions.Convert2dTo1d( position.X, position.Y, Width )] = true;
	}

	/// <summary>
	/// Check neighbours for dilation.
	/// </summary>
	/// <param name="positionsToDilate">The list of positions to check and dilate.</param>
	/// <param name="x">The x position we are dilating on.</param>
	/// <param name="z">The y position we are dilating on.</param>
	private void CheckForDilationPosition( ICollection<IntVector2> positionsToDilate, int x, int z )
	{
		if ( x - 1 > 0 )
			positionsToDilate.Add( new IntVector2( x - 1, z ) );
		if ( z - 1 > 0 )
			positionsToDilate.Add( new IntVector2( x, z - 1 ) );
		if ( x - 1 > 0 && z - 1 > 0 )
			positionsToDilate.Add( new IntVector2( x - 1, z - 1 ) );
		if ( x + 1 < Width )
			positionsToDilate.Add( new IntVector2( x + 1, z ) );
		if ( z + 1 < Height )
			positionsToDilate.Add( new IntVector2( x, z + 1 ) );
		if ( x + 1 < Width && z + 1 < Height )
			positionsToDilate.Add( new IntVector2( x + 1, z + 1 ) );
	}

	/// <summary>
	/// Destruct a sphere in the terrain grid.
	/// </summary>
	/// <param name="midpoint">The Vector2 midpoint of the sphere to be destructed.</param>
	/// <param name="size">The size (radius) of the sphere to be destructed.</param>
	/// <returns>Whether or not the terrain has been modified.</returns>
	public bool DestructCircle( Vector2 midpoint, float size )
	{
		var scaledMidpoint = midpoint / Scale;
		var centerIndex = Dimensions.Convert2dTo1d( (int)MathF.Round( scaledMidpoint.x, 0 ), (int)MathF.Round( scaledMidpoint.y, 0 ), Width );
		var (centerX, centerY) = Dimensions.Convert1dTo2d( centerIndex, Width );
		var distanceSquared = new Vector2( centerX, centerY ).DistanceSquared( scaledMidpoint );
		if ( distanceSquared > 1 )
			return false;

		var modifiedTerrain = false;
		var scaledSize = (int)Math.Round( size / Scale );

		foreach ( var index in GetIndexesInCircle( centerIndex, scaledSize ) )
		{
			if ( !TerrainGrid[index] )
				continue;

			_pendingDestroyedIndexes.Add( index );
			TerrainGrid[index] = false;
			var (x, y) = Dimensions.Convert1dTo2d( index, Width );
			modifiedTerrain |= TogglePointInChunks( x, y );
		}

		return modifiedTerrain;
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
			var pos = new Vector2( currentPoint.x, currentPoint.z );
			modifiedTerrain |= DestructCircle( pos, width );
		}

		return modifiedTerrain;
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="centerIndex"></param>
	/// <param name="radius"></param>
	/// <returns></returns>
	public IEnumerable<int> GetIndexesInCircle( int centerIndex, int radius )
	{
		var (xCenter, yCenter) = Dimensions.Convert1dTo2d( centerIndex, Width );
		for ( var x = xCenter - radius; x <= xCenter; x++ )
		{
			for ( var y = yCenter - radius; y <= yCenter; y++ )
			{
				if ( (x - xCenter) * (x - xCenter) + (y - yCenter) * (y - yCenter) > radius * radius )
					continue;

				var xSym = xCenter - (x - xCenter);
				var ySym = yCenter - (y - yCenter);

				var first = Dimensions.Convert2dTo1d( x, y, Width );
				if ( first >= 0 && first < TerrainGrid.Length )
					yield return first;

				var second = Dimensions.Convert2dTo1d( x, ySym, Width );
				if ( second >= 0 && second < TerrainGrid.Length )
					yield return second;

				var third = Dimensions.Convert2dTo1d( xSym, y, Width );
				if ( third >= 0 && third < TerrainGrid.Length )
					yield return third;

				var fourth = Dimensions.Convert2dTo1d( xSym, ySym, Width );
				if ( fourth >= 0 && fourth < TerrainGrid.Length )
					yield return fourth;
			}
		}
	}

	/// <summary>
	/// Get a random spawn location using Sandbox.Rand.
	/// Traces down to the ground to ensure Grubs do not take damage when spawned.
	/// </summary>
	/// <returns>A Vector3 position a Grub can be spawned at.</returns>
	public Vector3 GetSpawnLocation()
	{
		while ( true )
		{
			var x = Rand.Int( Width - 1 );
			var y = Rand.Int( Height - 1 );
			if ( TerrainGrid[Dimensions.Convert2dTo1d( x, y, Width )] )
				continue;

			var startPos = new Vector3( x * Scale, 0, y * Scale );
			var tr = Trace.Ray( startPos, startPos + Vector3.Down * Height * Scale ).WithTag( "solid" ).Run();
			if ( tr.Hit )
				return tr.EndPosition;
		}
	}

	private bool TogglePointInChunks( int x, int z )
	{
		var n = (x / ChunkSize) + (z / ChunkSize * (Width / ChunkSize));
		var xR = x % ChunkSize;
		var zR = z % ChunkSize;

		var chunk = TerrainGridChunks[n];
		if ( !chunk.TerrainGrid[xR, zR] )
			return false;

		chunk.TerrainGrid[xR, zR] = false;
		chunk.IsDirty = true;
		return true;
	}

	[Event.Tick.Server]
	private void ServerTick()
	{
		if ( _pendingDestroyedIndexes.Count == 0 )
			return;

		UpdateDestroyedIndexesRpc( To.Everyone, _pendingDestroyedIndexes.ToArray() );
		_pendingDestroyedIndexes.Clear();
	}

	[ClientRpc]
	public void UpdateGridRpc( bool[] terrainGrid )
	{
		TerrainGrid = terrainGrid;
		AssignGridToChunks();
		TerrainMain.Initialize();
	}

	[ClientRpc]
	private void UpdateDestroyedIndexesRpc( int[] destroyedIndexes )
	{
		foreach ( var index in destroyedIndexes )
		{
			TerrainGrid[index] = false;
			var (x, y) = Dimensions.Convert1dTo2d( index, Width );
			TogglePointInChunks( x, y );
		}

		TerrainMain.RefreshDirtyChunks();
	}

}
