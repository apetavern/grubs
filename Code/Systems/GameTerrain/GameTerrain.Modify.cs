using Sandbox.Sdf;

namespace Grubs.Terrain;

public partial class GameTerrain
{
	public void AddCircle( Vector2 center, float radius )
	{
		var circleSdf = new CircleSdf( center, radius );
		Add( SdfWorld, circleSdf, GenericMaterial );
	}

	public void SubtractCircle( Vector2 center, float radius )
	{
		var circleSdf = new CircleSdf( center, radius );
		Subtract( SdfWorld, circleSdf, GenericMaterial );
	}
	
	private void Add( Sdf2DWorld world, ISdf2D sdf, Sdf2DLayer layer )
	{
		sdf = sdf.Translate( new Vector2( 0, -WorldPosition.z ) );
		world.AddAsync( sdf, layer );
	}

	private void Subtract( Sdf2DWorld world, ISdf2D sdf, Sdf2DLayer layer )
	{
		sdf = sdf.Translate( new Vector2( 0, -WorldPosition.z ) );
		world.SubtractAsync( sdf, layer );
	}
}
