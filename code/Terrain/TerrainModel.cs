namespace Grubs.Terrain;

[Category( "Terrain" )]
public class TerrainModel : ModelEntity
{
	private MarchingSquares _marchingSquares = null!;
	private TerrainWallModel _wallModel = null!;

	public TerrainModel()
	{
		Tags.Add( "solid" );
	}

	public override void Spawn()
	{
		base.Spawn();

		GenerateMeshAndWalls();
	}

	public void GenerateMeshAndWalls()
	{
		_marchingSquares = new MarchingSquares();
		Model = _marchingSquares.GenerateModel();
		SetupPhysicsFromModel( PhysicsMotionType.Static );

		_wallModel?.Delete();
		_wallModel = new TerrainWallModel( _marchingSquares );
	}
}
