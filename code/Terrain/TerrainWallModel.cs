namespace Grubs.Terrain;

[Category( "Terrain" )]
public class TerrainWallModel : ModelEntity
{
	public TerrainWallModel()
	{
	}

	public TerrainWallModel( MarchingSquares marchingSquares )
	{
		Model = marchingSquares.CreateWallModel();
		Tags.Add( "solid" );

		SetupPhysicsFromModel( PhysicsMotionType.Static );
	}
}
