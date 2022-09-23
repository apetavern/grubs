namespace Grubs.Terrain;

/// <summary>
/// 
/// </summary>
[Category( "Terrain" )]
public sealed class TerrainModel : ModelEntity
{
	private readonly TerrainChunk _chunk = null!;
	private readonly TerrainWallModel _wallModel = null!;

	public TerrainModel()
	{
		Transmit = TransmitType.Never;
	}

	public TerrainModel( TerrainChunk chunk ) : this()
	{
		Tags.Add( "solid" );
		_chunk = chunk;
		_wallModel = new TerrainWallModel { Position = chunk.Position };

		RefreshModel();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		_wallModel.Delete();
	}

	/// <summary>
	/// 
	/// </summary>
	public void RefreshModel()
	{
		Position = _chunk.Position;

		var marchingSquares = new MarchingSquares();
		Model = marchingSquares.GenerateModel( _chunk );
		SetupPhysicsFromModel( PhysicsMotionType.Static );

		_wallModel.RefreshModel( _chunk, marchingSquares );
	}
}
