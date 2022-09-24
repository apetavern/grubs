using System.Collections.Immutable;
using Grubs.Utils;

namespace Grubs.Terrain;

/// <summary>
/// Represents a portion of a <see cref="TerrainMap"/>.
/// </summary>
[Category( "Terrain" )]
public sealed partial class TerrainChunk : ModelEntity
{
	/// <summary>
	/// The terrain that this chunk is a part of.
	/// </summary>
	[Net]
	public TerrainMap Map { get; private set; } = null!;

	/// <summary>
	/// The width of the chunk.
	/// </summary>
	[Net]
	public int Width { get; private set; }

	/// <summary>
	/// The height of the chunk.
	/// </summary>
	[Net]
	public int Height { get; private set; }

	/// <summary>
	/// The x neighbour of the chunk.
	/// </summary>
	[Net]
	public TerrainChunk XNeighbour { get; set; } = null!;

	/// <summary>
	/// The y neighbour of the chunk.
	/// </summary>
	[Net]
	public TerrainChunk YNeighbour { get; set; } = null!;

	/// <summary>
	/// The XY neighbour of the chunk.
	/// </summary>
	[Net]
	public TerrainChunk XyNeighbour { get; set; } = null!;

	/// <summary>
	/// A list of all the indices that this chunk represents.
	/// </summary>
	[Net]
	private IList<int> _containedIndices { get; set; } = null!;

	/// <summary>
	/// Gets the state of a position the chunk is holding.
	/// </summary>
	/// <param name="x">The x component to access.</param>
	/// <param name="y">The y component to access.</param>
	/// <param name="relative">Whether or not the <see cref="x"/> and <see cref="y"/> are relative to the chunks location.</param>
	/// <exception cref="ArgumentException"></exception>
	public bool this[int x, int y, bool relative = true]
	{
		get
		{
			var index = Dimensions.Convert2dTo1d( x, y, Map.Width );
			if ( relative )
				index += _containedIndices[0];

			if ( !_containedIndices.Contains( index ) )
				throw new ArgumentException( "Got invalid x, y to chunk accessor", $"{nameof( x )}, {nameof( y )}" );

			return Map.TerrainGrid[index];
		}
	}

	public TerrainChunk()
	{
		Transmit = TransmitType.Always;
	}

	public TerrainChunk( TerrainMap map, Vector3 position, int width, int height,
		IEnumerable<int> containedIndices ) : this()
	{
		Map = map;
		Position = position;
		Width = width;
		Height = height;

		foreach ( var index in containedIndices )
			_containedIndices.Add( index );

		Tags.Add( "solid" );
	}

	/// <summary>
	/// Refreshes the terrain chunks model
	/// </summary>
	public void RefreshModel()
	{
		if ( IsServer )
			RefreshModelRpc( To.Everyone );

		Model = new MarchingSquares().CreateModel( this );
		SetupPhysicsFromModel( PhysicsMotionType.Static );
	}

	/// <summary>
	/// Refreshes the model on the client-side.
	/// </summary>
	[ClientRpc]
	private void RefreshModelRpc()
	{
		RefreshModel();
	}
}
