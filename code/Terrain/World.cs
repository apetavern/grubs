using Sandbox.Csg;

namespace Grubs;

public partial class World : Entity
{
	[Net]
	public CsgSolid CsgWorld { get; set; }

	public CsgBrush CubeBrush { get; } = ResourceLibrary.Get<CsgBrush>( "brushes/cube.csg" );
	public CsgMaterial DefaultMaterial { get; } = ResourceLibrary.Get<CsgMaterial>( "materials/csg/default.csgmat" );

	public override void Spawn()
	{
		Assert.True( Game.IsServer );

		CsgWorld = new CsgSolid( 1024f );

		CsgWorld.Add( CubeBrush, DefaultMaterial, scale: new Vector3( 4092f, 64f, 1024f ), position: new Vector3( 0, 0, -512f ) );

		SubtractCube( -100, 100 );
	}

	public void SubtractCube( Vector3 min, Vector3 max )
	{
		CsgWorld.Subtract( CubeBrush, (min + max) * 0.5f, max - min );
	}
}
