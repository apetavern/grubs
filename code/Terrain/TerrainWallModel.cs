namespace Grubs.Terrain;

[Category( "Terrain" )]
public class TerrainWallModel : ModelEntity
{
	public TerrainWallModel( MarchingSquares marchingSquares )
	{
		Transmit = TransmitType.Never;
		Model = marchingSquares.CreateWallModel();
		Tags.Add( "solid" );

		SetupPhysicsFromModel( PhysicsMotionType.Static );
	}
}
