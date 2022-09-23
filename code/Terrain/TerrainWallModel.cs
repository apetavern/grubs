namespace Grubs.Terrain;

[Category( "Terrain" )]
public sealed class TerrainWallModel : ModelEntity
{
	public TerrainWallModel()
	{
		Transmit = TransmitType.Never;
	}

	public TerrainWallModel( TerrainMap map, MarchingSquares marchingSquares ) : this()
	{
		Model = marchingSquares.CreateWallModel( map );
		Tags.Add( "solid" );

		SetupPhysicsFromModel( PhysicsMotionType.Static );
	}
}
