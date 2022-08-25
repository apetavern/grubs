namespace Grubs.Terrain;

public class TerrainModel : ModelEntity
{
	public MarchingSquares marchingSquares = new();
	public ModelEntity WallModel;

	public override void Spawn()
	{
		base.Spawn();

		GenerateMeshAndWalls();
	}

	public void GenerateMeshAndWalls()
	{
		marchingSquares = new();
		Model = marchingSquares.GenerateModel();
		SetupPhysicsFromModel( PhysicsMotionType.Static );

		WallModel?.Delete();
		WallModel = new ModelEntity
		{
			Model = marchingSquares.CreateWallModel()
		};
		WallModel.SetupPhysicsFromModel( PhysicsMotionType.Static );
	}
}
