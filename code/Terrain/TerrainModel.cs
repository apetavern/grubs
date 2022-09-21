namespace Grubs.Terrain;

[Category( "Terrain" )]
public sealed class TerrainModel : ModelEntity
{
	private MarchingSquares _marchingSquares = null!;
	private TerrainWallModel _wallModel = null!;
	private readonly TerrainChunk _chunk = null!;

	public TerrainModel()
	{
	}
	public TerrainModel( TerrainChunk chunk ) : this()
	{
		Tags.Add( "solid" );
		_chunk = chunk;
		BuildMeshAndCollision();
	}
	public void BuildMeshAndCollision()
	{
		_marchingSquares = new MarchingSquares();
		Model = _marchingSquares.GenerateModel( _chunk );
		Position = _chunk.Position;

		SetupPhysicsFromModel( PhysicsMotionType.Static );

		_wallModel = new TerrainWallModel( _marchingSquares );
		_wallModel.Position = _chunk.Position;
	}

	// Weird hack since the old models don't delete properly?
	public void DestroyMeshAndCollision()
	{
		Model = null;
		_wallModel?.Delete();
		Position = new Vector3( -1000, -1000, -1000 );
	}
}
