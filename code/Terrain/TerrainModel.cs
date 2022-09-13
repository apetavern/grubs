using Grubs.Utils;

namespace Grubs.Terrain;

[Category( "Terrain" )]
public sealed class TerrainModel : ModelEntity
{
	public Entity Center = null!;

	private TerrainWallModel _wallModel = null!;

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
		var marchingSquares = new MarchingSquares();
		Model = marchingSquares.GenerateModel();
		SetupPhysicsFromModel( PhysicsMotionType.Static );

		var x = GameConfig.TerrainWidth * GameConfig.TerrainScale / 2;
		var z = GameConfig.TerrainHeight * GameConfig.TerrainScale / 2;
		Center = new Entity { Position = new Vector3( x, 0, z ) };

		_wallModel?.Delete();
		_wallModel = new TerrainWallModel( marchingSquares );
	}
}
