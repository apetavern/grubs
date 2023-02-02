using Sandbox.Csg;

namespace Grubs;

public partial class World : Entity
{
	[Net]
	public CsgSolid CsgWorld { get; set; }

	public CsgBrush CubeBrush { get; } = ResourceLibrary.Get<CsgBrush>( "brushes/cube.csg" );
	public CsgBrush CoolBrush { get; } = ResourceLibrary.Get<CsgBrush>( "brushes/cool.csg" );
	public CsgMaterial DefaultMaterial { get; } = ResourceLibrary.Get<CsgMaterial>( "materials/csg/default.csgmat" );
	public CsgMaterial SandMaterial { get; } = ResourceLibrary.Get<CsgMaterial>( "materials/csg/sand.csgmat" );

	private const float WorldLength = 4092f;
	private const float WorldWidth = 64f;
	private const float WorldHeight = 1024f;

	private const float GridSize = 1024f;

	public override void Spawn()
	{
		Assert.True( Game.IsServer );

		CsgWorld = new CsgSolid( GridSize );

		CsgWorld.Add( CubeBrush, SandMaterial, scale: new Vector3( WorldLength, WorldWidth, WorldHeight ), position: new Vector3( 0, 0, -WorldHeight / 2 ) );

		SubtractCube( -100, 100 );
	}

	public void SubtractCube( Vector3 min, Vector3 max )
	{
		CsgWorld.Subtract( CoolBrush, (min + max) * 0.5f, max - min );
	}

	public void GenerateRandomWorld()
	{

	}
}
