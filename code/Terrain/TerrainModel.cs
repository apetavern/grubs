namespace Grubs.Terrain;

[Category( "Terrain" )]
public class TerrainModel : ModelEntity
{
	public MarchingSquares MarchingSquares = null!;
	public TerrainWallModel WallModel = null!;

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
		MarchingSquares = new MarchingSquares();
		Model = MarchingSquares.GenerateModel();
		SetupPhysicsFromModel( PhysicsMotionType.Static );

		WallModel?.Delete();
		WallModel = new TerrainWallModel( MarchingSquares );
	}
}
