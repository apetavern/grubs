namespace Grubs.Terrain;

[Category( "Terrain" )]
public sealed class TerrainWallModel : ModelEntity
{
	public TerrainWallModel()
	{
		Transmit = TransmitType.Never;
	}

	public TerrainWallModel( MarchingSquares marchingSquares ) : this()
	{
		Model = marchingSquares.CreateWallModel();
		Tags.Add( "solid" );

		SetupPhysicsFromModel( PhysicsMotionType.Static );
	}
}
