using System.Collections.Immutable;
using System.Diagnostics;
using Grubs.Utils;
using Trace = Sandbox.Trace;

namespace Grubs.Terrain;

/// <summary>
/// A destructible terrain for Grubs to battle on.
/// </summary>
[Category( "Terrain" )]
public sealed partial class TerrainMap : Entity
{
	/// <summary>
	/// The seed that was used to generate the random terrain.
	/// <remarks>This will only be populated if the terrain was randomly generated.</remarks>
	/// </summary>
	[Net]
	public int Seed { get; private set; }

	/// <summary>
	/// Whether or not this terrain was pre-made
	/// </summary>
	[Net]
	public bool Premade { get; private set; }

	/// <summary>
	/// The pre-made map definition this terrain was created from.
	/// <remarks>This will only be populated on the server-side and when <see cref="Premade"/> is true.</remarks>
	/// </summary>
	public readonly PremadeTerrain? PremadeMap;

	/// <summary>
	/// The current grid state.
	/// </summary>
	public bool[] TerrainGrid { get; private set; } = Array.Empty<bool>();

	/// <summary>
	/// The width of the terrain grid.
	/// </summary>
	[Net]
	public int Width { get; private set; }

	/// <summary>
	/// The height of the terrain grid.
	/// </summary>
	[Net]
	public int Height { get; private set; }

	/// <summary>
	/// The world scale for the terrain.
	/// </summary>
	[Net]
	public new int Scale { get; private set; }

	/// <summary>
	/// Whether or not the terrain has a border on it.
	/// </summary>
	[Net]
	public bool HasBorder { get; private set; }

	/// <summary>
	/// The type of terrain this terrain is.
	/// </summary>
	[Net]
	public TerrainType TerrainType { get; private set; }

	public readonly List<TerrainChunk> TerrainGridChunks = new();
	private readonly List<TerrainModel> _terrainModels = new();
	private readonly List<int> _pendingIndices = new();
	private readonly List<bool> _pendingIndexValues = new();
	private readonly HashSet<int> _dirtyChunks = new();
	private TimeSince _timeSinceLastDebug;

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
		InitializeModels();
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
		InitializeModels();
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

	/// <summary>
	/// Assigns portions of the terrain grid to chunks.
	/// </summary>
	private void AssignGridToChunks()
	{
		var chunkCount = (Width * Height) / (ChunkSize * ChunkSize);

		var xOffset = 0;
		var yOffset = 0;
		for ( var i = 0; i < chunkCount; i++ )
		{
			var chunkPos = new Vector3( xOffset * Scale, 0, yOffset * Scale );

			var containedIndices = ImmutableArray.CreateBuilder<int>();
			for ( var x = xOffset; x < xOffset + ChunkSize; x++ )
				for ( var y = yOffset; y < yOffset + ChunkSize; y++ )
					containedIndices.Add( Dimensions.Convert2dTo1d( x, y, Width ) );

			var chunk = new TerrainChunk( this, chunkPos, ChunkSize, ChunkSize, containedIndices.ToImmutable() );

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

	/// <summary>
	/// Adds a border around the edge of the terrain.
	/// </summary>
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
	/// <remarks>https://en.wikipedia.org/wiki/Connected-component_labeling</remarks>
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
	/// Edits a sphere in the terrain grid.
	/// </summary>
	/// <param name="midpoint">The center of the sphere to be edited.</param>
	/// <param name="size">The radius of the sphere to be edited.</param>
	/// <param name="mode">The way the circle should be edited.</param>
	/// <returns>Whether or not the terrain has been edited.</returns>
	public bool EditCircle( Vector2 midpoint, float size, TerrainModifyMode mode )
	{
		Host.AssertServer();

		var scaledMidpoint = midpoint / Scale;
		var centerIndex = Dimensions.Convert2dTo1d( (int)MathF.Round( scaledMidpoint.x, 0 ), (int)MathF.Round( scaledMidpoint.y, 0 ), Width );
		// Reconstruct the midpoint to check if converting it wrapped to an invalid value.
		var (centerX, centerY) = Dimensions.Convert1dTo2d( centerIndex, Width );
		var distanceSquared = new Vector2( centerX, centerY ).DistanceSquared( scaledMidpoint );
		if ( distanceSquared > 1 )
			return false;

		var modifiedTerrain = false;
		var scaledSize = (int)Math.Round( size / Scale );

		foreach ( var index in GetIndicesInCircle( centerIndex, scaledSize ) )
			modifiedTerrain |= EditPoint( index, mode );

		return modifiedTerrain;
	}

	/// <summary>
	/// Edits a line in the terrain grid.
	/// </summary>
	/// <param name="startPoint">The start point of the line to be edited.</param>
	/// <param name="endPoint">The end point of the line to be edited.</param>
	/// <param name="width">The radius of the line spheres to be edited.</param>
	/// <param name="mode">The way the line should be edited.</param>
	/// <returns>Whether or not the terrain has been edited.</returns>
	public bool EditLine( Vector2 startPoint, Vector2 endPoint, float width, TerrainModifyMode mode )
	{
		Host.AssertServer();

		var totalLength = (startPoint - endPoint);
		var stepCount = (int)MathF.Round( totalLength.Length / width );
		var modifiedTerrain = false;

		for ( var i = 0; i < stepCount; i++ )
		{
			var currentPoint = Vector3.Lerp( startPoint, endPoint, (float)i / stepCount );
			var pos = new Vector2( currentPoint.x, currentPoint.z );
			modifiedTerrain |= EditCircle( pos, width, mode );
		}

		return modifiedTerrain;
	}

	/// <summary>
	/// Edits a single point in the terrain grid.
	/// </summary>
	/// <param name="x">The x component of the point.</param>
	/// <param name="y">The y component of the point.</param>
	/// <param name="mode">The way the point should be edited.</param>
	/// <returns>Whether or not the terrain has been edited.</returns>
	public bool EditPoint( int x, int y, TerrainModifyMode mode )
	{
		Host.AssertServer();

		return EditPoint( Dimensions.Convert2dTo1d( x, y, Width ), mode );
	}

	/// <summary>
	/// Edits a single point in the terrain grid.
	/// </summary>
	/// <param name="index">The index to edit.</param>
	/// <param name="mode">The way the point should be edited.</param>
	/// <returns>Whether or not the terrain has been edited.</returns>
	public bool EditPoint( int index, TerrainModifyMode mode )
	{
		Host.AssertServer();

		if ( TerrainGrid[index] && mode == TerrainModifyMode.Add )
			return false;

		if ( !TerrainGrid[index] && mode == TerrainModifyMode.Remove )
			return false;

		if ( IsServer )
		{
			_pendingIndices.Add( index );
			_pendingIndexValues.Add( !TerrainGrid[index] );
		}

		TerrainGrid[index] = !TerrainGrid[index];
		var (x, y) = Dimensions.Convert1dTo2d( index, Width );
		var n = (x / ChunkSize) + (y / ChunkSize * (Width / ChunkSize));
		_dirtyChunks.Add( n );
		return true;
	}

	/// <summary>
	/// Gets all indices that are enveloped by a circle.
	/// </summary>
	/// <param name="x">The x component of the center point.</param>
	/// <param name="y">The y component of the center point.</param>
	/// <param name="radius">The radius of the circle in grid points.</param>
	/// <returns>All the indices that are inside the circle.</returns>
	public IEnumerable<int> GetIndicesInCircle( int x, int y, int radius )
	{
		return GetIndicesInCircle( Dimensions.Convert2dTo1d( x, y, Width ), radius );
	}

	/// <summary>
	/// Gets all indices that are enveloped by a circle.
	/// <remarks>https://stackoverflow.com/questions/15856411/finding-all-the-points-within-a-circle-in-2d-space</remarks>
	/// </summary>
	/// <param name="centerIndex">The center point of the circle.</param>
	/// <param name="radius">The radius of the circle in grid points.</param>
	/// <returns>All the indices that are inside the circle.</returns>
	public IEnumerable<int> GetIndicesInCircle( int centerIndex, int radius )
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
	/// Gets all indices that are within a rectangle.
	/// </summary>
	/// <param name="x">The x component of the center point.</param>
	/// <param name="y">The y component of the center point.</param>
	/// <param name="rectSize">The size of the rectangle to check.</param>
	/// <returns>All the indices that are inside the rectangle.</returns>
	public IEnumerable<int> GetIndicesInRect( int x, int y, IntVector2 rectSize )
	{
		return GetIndicesInRect( Dimensions.Convert2dTo1d( x, y, Width ), rectSize );
	}

	/// <summary>
	/// Gets all indices that are within a rectangle.
	/// <remarks>This will fail if <see cref="rectSize"/>.<see cref="IntVector2.X"/> is even.</remarks>
	/// </summary>
	/// <param name="centerIndex">The center point of the rectangle.</param>
	/// <param name="rectSize">The size of the rectangle to check.</param>
	/// <returns>All the indices that are inside the rectangle.</returns>
	public IEnumerable<int> GetIndicesInRect( int centerIndex, IntVector2 rectSize )
	{
		Assert.True( rectSize.X % 2 == 0, $"{nameof( rectSize.X )} must be odd" );

		var bottomLeftIndex = centerIndex - Dimensions.Convert2dTo1d( rectSize.X, rectSize.Y, Width ) / 2;
		for ( var i = 0; i < rectSize.X * rectSize.Y; i++ )
		{
			var (x, y) = Dimensions.Convert1dTo2d( i, rectSize.X );
			var index = bottomLeftIndex + Dimensions.Convert2dTo1d( x, y, Width );
			if ( index >= 0 && index < TerrainGrid.Length )
				yield return index;
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

	/// <summary>
	/// Initializes the terrain physics and models.
	/// </summary>
	private void InitializeModels()
	{
		Host.AssertServer();

		foreach ( var model in _terrainModels )
			model.Delete();
		_terrainModels.Clear();

		for ( var i = 0; i < TerrainGridChunks.Count; i++ )
			_terrainModels.Add( new TerrainModel( this, i ) );
	}

	/// <summary>
	/// Refreshes any chunks that have been edited.
	/// </summary>
	private void RefreshDirtyChunks()
	{
		Host.AssertServer();

		foreach ( var index in _dirtyChunks )
			_terrainModels[index].RefreshModel();

		_dirtyChunks.Clear();
	}

	/// <summary>
	/// Checks if any indices have been destroyed and update if there are.
	/// </summary>
	[Event.Tick.Server]
	private void ServerTick()
	{
		if ( TerrainDebugLevel != 0 && _timeSinceLastDebug > 0.5 )
		{
			switch ( TerrainDebugLevel )
			{
				case 2:
					DrawGridDebug();
					DrawChunkDebug();
					break;
				case 1:
					DrawChunkDebug();
					break;
			}
			_timeSinceLastDebug = 0;
		}

		if ( _pendingIndices.Count == 0 )
			return;

		UpdateDestroyedIndicesRpc( To.Everyone, _pendingIndices.ToArray(), _pendingIndexValues.ToArray() );
		_pendingIndices.Clear();
		_pendingIndexValues.Clear();
		RefreshDirtyChunks();
	}

	/// <summary>
	/// Draws the grid array. If <see cref="TerrainDebugLevel"/> is 3 then empty positions will also be shown.
	/// </summary>
	private void DrawGridDebug()
	{
		for ( var i = 0; i < TerrainGrid.Length; i++ )
		{
			var (x, y) = Dimensions.Convert1dTo2d( i, Width );
			var mins = new Vector3( x * Scale, -31, y * Scale );
			var maxs = new Vector3( mins.x + Scale, -32, mins.z + Scale );
			if ( TerrainGrid[i] )
				DebugOverlay.Box( mins, maxs.WithY( -33 ), Color.Green, 0.6f );
			else if ( TerrainDebugLevel == 3 )
				DebugOverlay.Box( mins, maxs, Color.Red, 0.6f );
		}
	}

	/// <summary>
	/// Draws outlines on all chunks.
	/// </summary>
	private void DrawChunkDebug()
	{
		foreach ( var chunk in TerrainGridChunks )
		{
			var mins = chunk.Position.WithY( -29 );
			DebugOverlay.Box( mins, new Vector3( mins.x + chunk.Width * Scale, -30, mins.z + chunk.Height * Scale ), Color.Yellow, 0.6f );
		}
	}

	/// <summary>
	/// Sends the current terrain grid state to a client.
	/// </summary>
	/// <param name="terrainGrid">The terrain grid state.</param>
	[ClientRpc]
	public void UpdateGridRpc( bool[] terrainGrid )
	{
		TerrainGrid = terrainGrid;
		AssignGridToChunks();
	}

	/// <summary>
	/// Sends any indices that have been edited to a client.
	/// </summary>
	/// <param name="editedIndices">The indices that have been edited.</param>
	/// <param name="indexValues">The values of the indices that were edited.</param>
	[ClientRpc]
	private void UpdateDestroyedIndicesRpc( int[] editedIndices, bool[] indexValues )
	{
		for ( var i = 0; i < editedIndices.Length; i++ )
			TerrainGrid[editedIndices[i]] = indexValues[i];
	}

	/// <summary>
	/// Debug console variable to see debug information relating to terrain.
	/// </summary>
	[ConVar.Server( "terrain_debug_level", Min = 0, Max = 3 )]
	public static int TerrainDebugLevel { get; set; } = 0;
}
