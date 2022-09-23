using Grubs.States;

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
	[Net]
	private TerrainWallModel _wallModel { get; set; } = null!;

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
		_wallModel = new TerrainWallModel { Position = Chunk.Position };

		RefreshModel();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		if ( IsServer )
			_wallModel.Delete();
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

		var marchingSquares = new MarchingSquares();
		Model = marchingSquares.GenerateModel( Chunk );
		SetupPhysicsFromModel( PhysicsMotionType.Static );

		_wallModel.RefreshModel( Chunk, marchingSquares );
	}

	[ClientRpc]
	private void RefreshModelRpc()
	{
		RefreshModel();
	}
}
