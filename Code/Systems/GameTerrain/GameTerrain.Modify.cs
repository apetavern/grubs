using Sandbox.Sdf;

namespace Grubs.Terrain;

public partial class GameTerrain
{
	public void AddCircle( Vector2 center, float radius )
	{
		var circleSdf = new CircleSdf( center, radius );
		Add( SdfWorld, circleSdf, _activeLayer );
	}

	public void AddBox( Vector2 center, Vector2 size )
	{
		var boxSdf = new RectSdf( center - size / 2f, center + size / 2f );
		Add( SdfWorld, boxSdf, _activeLayer );
	}

	public void SubtractCircle( Vector2 center, float radius )
	{
		var circleSdf = new CircleSdf( center, radius );
		Subtract( SdfWorld, circleSdf, _activeLayer );
	}

	public void SubtractBox( Vector2 center, Vector2 size )
	{
		var boxSdf = new RectSdf( center - size / 2f, center + size / 2f );
		Subtract( SdfWorld, boxSdf, _activeLayer );
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
