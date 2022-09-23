namespace Grubs.Terrain;

/// <summary>
/// 
/// </summary>
[Category( "Terrain" )]
public sealed partial class TerrainModel : ModelEntity
{
	[Net]
	private TerrainMap Map { get; set; } = null!;
	[Net]
	private int ChunkIndex { get; set; }

	private TerrainChunk Chunk => Map.TerrainGridChunks[ChunkIndex];

	public TerrainModel()
	{
		Transmit = TransmitType.Always;
	}

	public TerrainModel( TerrainMap map, int chunkIndex ) : this()
	{
		Map = map;
		ChunkIndex = chunkIndex;

		Tags.Add( "solid" );

		RefreshModel();
	}

	/// <summary>
	/// 
	/// </summary>
	public void RefreshModel()
	{
		if ( IsServer )
		{
			RefreshModelRpc( To.Everyone );
			Position = Chunk.Position;
		}

		Model = new MarchingSquares().CreateModel( Chunk );
		SetupPhysicsFromModel( PhysicsMotionType.Static );
	}

	[ClientRpc]
	private void RefreshModelRpc()
	{
		RefreshModel();
	}
}
