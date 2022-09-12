using Grubs.Utils;

namespace Grubs.Terrain;

[Category( "Terrain" )]
public sealed class TerrainModel : ModelEntity
{
	public Entity Center = null!;

	private TerrainWallModel _wallModel = null!;
	private TerrainChunk _chunk;

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
		Model = _marchingSquares.GenerateModel( _chunk.TerrainGrid );
		Position = _chunk.Position;

		SetupPhysicsFromModel( PhysicsMotionType.Static );

		_wallModel = new TerrainWallModel( _marchingSquares );
		_wallModel.Position = _chunk.Position;
	}
}
