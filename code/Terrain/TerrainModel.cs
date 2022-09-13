namespace Grubs.Terrain;

[Category( "Terrain" )]
public sealed class TerrainModel : ModelEntity
{
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
		var marchingSquares = new MarchingSquares();
		Model = marchingSquares.GenerateModel();
		SetupPhysicsFromModel( PhysicsMotionType.Static );

		_wallModel?.Delete();
		_wallModel = new TerrainWallModel( marchingSquares );
	}
}
