using System.Collections.Immutable;
using Grubs.Utils;

namespace Grubs.Terrain;

/// <summary>
/// Represents a portion of a <see cref="TerrainMap"/>.
/// </summary>
[Category( "Terrain" )]
public sealed class TerrainChunk
{
	/// <summary>
	/// The terrain that this chunk is a part of.
	/// </summary>
	public readonly TerrainMap Map;

	/// <summary>
	/// The world position of the chunk.
	/// </summary>
	public readonly Vector3 Position;

	/// <summary>
	/// The width of the chunk.
	/// </summary>
	public readonly int Width;

	/// <summary>
	/// The height of the chunk.
	/// </summary>
	public readonly int Height;

	/// <summary>
	/// The x neighbour of the chunk.
	/// </summary>
	public TerrainChunk XNeighbour = null!;

	/// <summary>
	/// The y neighbour of the chunk.
	/// </summary>
	public TerrainChunk YNeighbour = null!;

	/// <summary>
	/// The XY neighbour of the chunk.
	/// </summary>
	public TerrainChunk XyNeighbour = null!;

	/// <summary>
	/// An array of all the indices that this chunk represents.
	/// </summary>
	private readonly ImmutableArray<int> _containedIndices;

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

	public TerrainChunk( TerrainMap map, Vector3 position, int width, int height, ImmutableArray<int> containedIndices )
	{
		Map = map;
		Position = position;
		Width = width;
		Height = height;
		_containedIndices = containedIndices;
	}
}
