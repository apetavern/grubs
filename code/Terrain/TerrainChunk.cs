namespace Grubs.Terrain;

[Category( "Terrain" )]
public sealed class TerrainChunk
{
	public readonly TerrainMap Map;

	public Vector3 Position { get; }
	public bool[,] TerrainGrid { get; init; } = null!;

	public TerrainChunk XNeighbour = null!;
	public TerrainChunk YNeighbour = null!;
	public TerrainChunk XyNeighbour = null!;
	public bool IsDirty { get; set; }

	public TerrainChunk( Vector3 position, TerrainMap map )
	{
		Map = map;
		Position = position;
	}
}
