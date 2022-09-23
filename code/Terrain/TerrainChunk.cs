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
	public Vector3 Position { get; }
	/// <summary>
	/// The internal terrain grid this chunk represents.
	/// </summary>
	public bool[,] TerrainGrid { get; init; } = null!;

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
	/// Whether or not this chunk has been changed.
	/// </summary>
	public bool IsDirty { get; set; }

	public TerrainChunk( Vector3 position, TerrainMap map )
	{
		Map = map;
		Position = position;
	}
}
