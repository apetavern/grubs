using Sandbox.Sdf;

namespace Grubs;

public partial class Terrain
{
	/// <summary>
	/// Wrapper for a standard circle subtraction.
	/// </summary>
	/// <param name="center">The Vector2 center of the subtraction.</param>
	/// <param name="radius">The radius of the subtraction.</param>
	public void SubtractCircle( Vector2 center, float radius )
	{
		var circleSdf = new CircleSdf( center, radius );
		Subtract( SdfWorld, circleSdf, DevMaterial );
	}

	/// <summary>
	/// Wrapper for a standard box subtraction.
	/// </summary>
	/// <param name="mins">The minimum bounds for the box.</param>
	/// <param name="maxs">The maximum bounds for the box.</param>
	public void SubtractBox( Vector2 mins, Vector2 maxs)
	{
		var boxSdf = new BoxSdf( mins, maxs );
		Subtract( SdfWorld, boxSdf, DevMaterial );
	}

	/// <summary>
	/// Creates the standard world box for generated terrain.
	/// </summary>
	/// <param name="length">The length of the world.</param>
	/// <param name="height">The height of the world.</param>
	private void AddWorldBox( int length, int height )
	{
		var boxSdf = new BoxSdf( new Vector2( -length / 2, 0 ), new Vector2( length / 2, height ) );
		Add( SdfWorld, boxSdf, DevMaterial );
	}

	/// <summary>
	/// Wrapper for a standard Sdf addition.
	/// </summary>
	/// <param name="world">The world to subtract from.</param>
	/// <param name="sdf">The Sdf to apply.</param>
	/// <param name="material">The material to apply.</param>
	private void Add( Sdf2DWorld world, ISdf2D sdf, Sdf2DMaterial material )
	{
		world.Add( sdf, material );
	}

	/// <summary>
	/// Wrapper for a standard Sdf subtraction.
	/// </summary>
	/// <param name="world">The world to subtract from.</param>
	/// <param name="sdf">The Sdf to apply.</param>
	/// <param name="material">The material to apply.</param>
	private void Subtract( Sdf2DWorld world, ISdf2D sdf, Sdf2DMaterial material )
	{
		world.Subtract( sdf, material );
	}
}
