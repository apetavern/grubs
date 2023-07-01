using Sandbox.Sdf;

namespace Grubs;

public partial class Terrain
{
	private int lengthOffset;
	private int heightOffset;

	/// <summary>
	/// Wrapper for a standard circle subtraction.
	/// </summary>
	/// <param name="center">The Vector2 center of the subtraction.</param>
	/// <param name="radius">The radius of the subtraction.</param>
	/// <param name="materials">The Sdf2dMaterials and offsets of the subtraction.</param>
	/// <param name="worldOffset">Whether or not to use an offset from the Sdf to the world (for terrain generation).</param>
	public void SubtractCircle( Vector2 center, float radius, Dictionary<Sdf2DLayer, float> materials, bool worldOffset = false )
	{
		var circleSdf = new CircleSdf( center, radius );
		foreach ( var (material, offset) in materials )
			Subtract( SdfWorld, circleSdf.Expand( offset ), material, offset: worldOffset );
	}

	/// <summary>
	/// Scorches a circle shape.
	/// </summary>
	/// <param name="center">The Vector2 center of the scorch.</param>
	/// <param name="radius">The radius of the scorch.</param>
	public void ScorchCircle( Vector2 center, float radius )
	{
		var circleSdf = new CircleSdf( center, radius );
		Add( SdfWorld, circleSdf, ScorchMaterial );
	}

	/// <summary>
	/// Wrapper for a standard box subtraction.
	/// </summary>
	/// <param name="mins">The minimum bounds for the box.</param>
	/// <param name="maxs">The maximum bounds for the box.</param>
	/// <param name="materials">The Sdf2dMaterials and offsets of the subtraction.</param>
	/// <param name="cornerRadius">The corner radius of the box.</param>
	/// <param name="worldOffset">Whether or not to use an offset from the Sdf to the world (for terrain generation).</param>
	public void SubtractBox( Vector2 mins, Vector2 maxs, Dictionary<Sdf2DLayer, float> materials, float cornerRadius = 0, bool worldOffset = false )
	{
		var boxSdf = new RectSdf( mins, maxs, cornerRadius );
		foreach ( var (material, offset) in materials )
			Subtract( SdfWorld, boxSdf.Expand( offset ), material, offset: worldOffset );
	}

	/// <summary>
	/// Wrapper for a standard line subtraction.
	/// </summary>
	/// <param name="start">The start point of the line.</param>
	/// <param name="end">The end point of the line.</param>
	/// <param name="radius">The radius of the line.</param>
	/// <param name="materials">The Sdf2dMaterials and offsets of the subtraction.</param>
	/// <param name="worldOffset">Whether or not to use an offset from the Sdf to the world (for terrain generation).</param>
	public void SubtractLine( Vector2 start, Vector2 end, float radius, Dictionary<Sdf2DLayer, float> materials, bool worldOffset = false )
	{
		var lineSdf = new LineSdf( start, end, radius );
		foreach ( var (material, offset) in materials )
			Subtract( SdfWorld, lineSdf.Expand( offset ), material, offset: worldOffset );
	}

	public void ScorchLine( Vector2 start, Vector2 end, float radius )
	{
		var lineSdf = new LineSdf( start, end, radius );
		Add( SdfWorld, lineSdf, ScorchMaterial );
	}

	/// <summary>
	/// Wrapper for a texture addition.
	/// </summary>
	/// <param name="texture">The Texture to add to the world.</param>
	/// <param name="gradientWidth">The gradient width of the texture.</param>
	/// <param name="worldWidth">The width of the texture.</param>
	/// <param name="position">The position of the Sdf.</param>
	/// <param name="rotation">The rotation of the Sdf.</param>
	/// <param name="materials">The Sdf2dMaterials and offsets of the subtraction.</param>
	public void AddTexture( Texture texture, int gradientWidth, float worldWidth, Vector2 position, Rotation2D rotation, Dictionary<Sdf2DLayer, float> materials )
	{
		var textureSdf = new TextureSdf( texture, gradientWidth, worldWidth );
		var transformedTextureSdf = textureSdf
			.Transform( position, rotation );
		foreach ( var (material, offset) in materials )
			Add( SdfWorld, transformedTextureSdf.Expand( offset ), material );
	}

	/// <summary>
	/// Creates the standard world box for generated terrain.
	/// </summary>
	/// <param name="length">The length of the world.</param>
	/// <param name="height">The height of the world.</param>
	/// <param name="fgMaterial">The material for the foreground of the world.</param>
	/// <param name="bgMaterial">The material for the background of the world.</param>
	private void AddWorldBox( int length, int height, Sdf2DLayer fgMaterial, Sdf2DLayer bgMaterial )
	{
		lengthOffset = length / 2;
		heightOffset = 0;

		var boxSdf = new RectSdf( new Vector2( -length / 2, 0 ), new Vector2( length / 2, height ) );
		Add( SdfWorld, boxSdf, fgMaterial );
		Add( SdfWorld, boxSdf, bgMaterial );
	}

	/// <summary>
	/// Wrapper for a standard Sdf addition.
	/// </summary>
	/// <param name="world">The world to subtract from.</param>
	/// <param name="sdf">The Sdf to apply.</param>
	/// <param name="material">The material to apply.</param>
	private void Add( Sdf2DWorld world, ISdf2D sdf, Sdf2DLayer material )
	{
		sdf = sdf.Translate( new Vector2( 0, -world.Position.z ) );
		world.AddAsync( sdf, material );
	}

	/// <summary>
	/// Wrapper for a standard Sdf subtraction.
	/// </summary>
	/// <param name="world">The world to subtract from.</param>
	/// <param name="sdf">The Sdf to apply.</param>
	/// <param name="material">The material to apply.</param>
	/// <param name="offset">Whether to apply the offset of the Sdf to world position.</param>
	private void Subtract( Sdf2DWorld world, ISdf2D sdf, Sdf2DLayer material, bool offset = false )
	{
		sdf = sdf.Translate( new Vector2( 0, -world.Position.z ) );
		if ( offset )
			sdf = sdf.Translate( new Vector2( -lengthOffset, heightOffset ) );
		world.SubtractAsync( sdf, material );
	}
}
