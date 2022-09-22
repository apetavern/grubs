namespace Grubs.Terrain;

[Category( "Terrain" )]
public sealed class TerrainModel : ModelEntity
{
	private readonly TerrainChunk _chunk = null!;
	private TerrainWallModel? _wallModel;

	public TerrainModel()
	{
		Transmit = TransmitType.Never;
	}

	public TerrainModel( TerrainChunk chunk ) : this()
	{
		Tags.Add( "solid" );
		_chunk = chunk;
		BuildMeshAndCollision();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		_wallModel?.Delete();
	}

	public void BuildMeshAndCollision()
	{
		var marchingSquares = new MarchingSquares();
		Model = marchingSquares.GenerateModel( _chunk );
		Position = _chunk.Position;

		SetupPhysicsFromModel( PhysicsMotionType.Static );

		_wallModel = new TerrainWallModel( marchingSquares ) { Position = _chunk.Position };
	}

	// Weird hack since the old models don't delete properly?
	public void DestroyMeshAndCollision()
	{
		Model = null;
		_wallModel?.Delete();
		Position = new Vector3( -1000, -1000, -1000 );
	}
}
