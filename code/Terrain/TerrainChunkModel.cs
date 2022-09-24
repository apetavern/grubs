namespace Grubs.Terrain;

/// <summary>
/// Contains the physics and visuals of a chunk in the terrain.
/// </summary>
[Category( "Terrain" )]
public sealed partial class TerrainModel : ModelEntity
{
	/// <summary>
	/// The map that this terrain model is a part of.
	/// </summary>
	[Net]
	private TerrainMap Map { get; set; } = null!;

	/// <summary>
	/// The index of the chunk that this model is representing.
	/// </summary>
	[Net]
	private int ChunkIndex { get; set; }

	/// <summary>
	/// The chunk that this model is representing.
	/// </summary>
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
	/// Refreshes the terrain chunks model
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

	/// <summary>
	/// Refreshes the model on the client-side.
	/// </summary>
	[ClientRpc]
	private void RefreshModelRpc()
	{
		RefreshModel();
	}
}
