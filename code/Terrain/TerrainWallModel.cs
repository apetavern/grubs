namespace Grubs.Terrain;

[Category( "Terrain" )]
public sealed class TerrainWallModel : ModelEntity
{
	public TerrainWallModel()
	{
		Transmit = TransmitType.Never;
	}

	public TerrainWallModel( TerrainChunk chunk, MarchingSquares marchingSquares ) : this()
	{
		Model = marchingSquares.CreateWallModel( chunk );
		Tags.Add( "solid" );

		SetupPhysicsFromModel( PhysicsMotionType.Static );
	}
}
