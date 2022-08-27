namespace Grubs.Terrain;

public class TerrainModel : ModelEntity
{
	public MarchingSquares marchingSquares = new();
	public ModelEntity WallModel;

	public TerrainModel()
	{
		Transmit = TransmitType.Never;
	}

	public override void Spawn()
	{
		base.Spawn();

		Tags.Add( "solid" );
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
			Model = marchingSquares.CreateWallModel(),
			Transmit = TransmitType.Never
		};
		WallModel.SetupPhysicsFromModel( PhysicsMotionType.Static );
		WallModel.Tags.Add( "solid" );
	}
}
