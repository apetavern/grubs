namespace Grubs.Terrain;

/// <summary>
/// 
/// </summary>
[Category( "Terrain" )]
public sealed class TerrainWallModel : ModelEntity
{
	public TerrainWallModel()
	{
		Transmit = TransmitType.Always;
	}

	public override void Spawn()
	{
		base.Spawn();

		Tags.Add( "solid" );
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="chunk"></param>
	/// <param name="marchingSquares"></param>
	public void RefreshModel( TerrainChunk chunk, MarchingSquares marchingSquares )
	{
		Model = marchingSquares.CreateWallModel( chunk );
		SetupPhysicsFromModel( PhysicsMotionType.Static );
	}
}
