namespace Grubs.Terrain;

public class TerrainModel : ModelEntity
{
	public MarchingSquares marchingSquares = new();

	public override void Spawn()
	{
		base.Spawn();

		GenerateMeshAndWalls();
	}

	public override void ClientSpawn()
	{
		base.ClientSpawn();

		GenerateMeshAndWalls();
	}

	public void GenerateMeshAndWalls()
	{
		Model = marchingSquares.GenerateModel();
		SetupPhysicsFromModel( PhysicsMotionType.Static );

		var wallModel = new ModelEntity
		{
			Model = marchingSquares.CreateWallModel()
		};
		wallModel.SetupPhysicsFromModel( PhysicsMotionType.Static );
	}
}
