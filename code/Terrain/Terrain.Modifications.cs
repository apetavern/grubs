using Sandbox.Sdf;

namespace Grubs.Terrain;

public partial class GrubsTerrain
{
	private int _lengthOffset;
	private int _heightOffset;

	private MaterialsConfig GetMaterialsFromCode( int code )
	{
		return code switch
		{
			0 => MaterialsConfig.Default,
			1 => MaterialsConfig.Destruction,
			2 => MaterialsConfig.DestructionWithBackground,
			_ => MaterialsConfig.Default
		};
	}

	/// <summary>
	/// Wrapper for a standard circle subtraction.
	/// </summary>
	/// <param name="center">The Vector2 center of the subtraction.</param>
	/// <param name="radius">The radius of the subtraction.</param>
	/// <param name="matCode">The Sdf2dMaterials and offsets of the subtraction.</param>
	/// <param name="worldOffset">Whether or not to use an offset from the Sdf to the world (for terrain generation).</param>
	[Rpc.Broadcast]
	public void SubtractCircle( Vector2 center, float radius, int matCode, bool worldOffset = false )
	{
		if ( !AssertIsHost() )
			return;

		var matConfig = GetMaterialsFromCode( matCode );
		var activeMaterials = GetActiveMaterials( matConfig );

		var circleSdf = new CircleSdf( center, radius );
		foreach ( var (material, offset) in activeMaterials )
			Subtract( SdfWorld, circleSdf.Expand( offset ), material, worldOffset );
	}

	/// <summary>
	/// Scorches a circle shape.
	/// </summary>
	/// <param name="center">The Vector2 center of the scorch.</param>
	/// <param name="radius">The radius of the scorch.</param>
	[Rpc.Broadcast]
	public void ScorchCircle( Vector2 center, float radius )
	{
		if ( !AssertIsHost() )
			return;

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
	[Rpc.Broadcast]
	public void SubtractBox( Vector2 mins, Vector2 maxs, int matCode,
		float cornerRadius = 0, bool worldOffset = false )
	{
		if ( !AssertIsHost() )
			return;

		var matConfig = GetMaterialsFromCode( matCode );
		var activeMaterials = GetActiveMaterials( matConfig );

		var boxSdf = new RectSdf( mins, maxs, cornerRadius );
		foreach ( var (material, offset) in activeMaterials )
			Subtract( SdfWorld, boxSdf.Expand( offset ), material, worldOffset );
	}

	/// <summary>
	/// Wrapper for a standard line subtraction.
	/// </summary>
	/// <param name="start">The start point of the line.</param>
	/// <param name="end">The end point of the line.</param>
	/// <param name="radius">The radius of the line.</param>
	/// <param name="materials">The Sdf2dMaterials and offsets of the subtraction.</param>
	/// <param name="worldOffset">Whether or not to use an offset from the Sdf to the world (for terrain generation).</param>
	[Rpc.Broadcast]
	public void SubtractLine( Vector2 start, Vector2 end, float radius, int matCode,
		bool worldOffset = false )
	{
		if ( !AssertIsHost() )
			return;

		var matConfig = GetMaterialsFromCode( matCode );
		var activeMaterials = GetActiveMaterials( matConfig );

		var lineSdf = new LineSdf( start, end, radius );
		foreach ( var (material, offset) in activeMaterials )
			Subtract( SdfWorld, lineSdf.Expand( offset ), material, worldOffset );
	}

	[Rpc.Broadcast]
	public void ScorchLine( Vector2 start, Vector2 end, float radius )
	{
		if ( !AssertIsHost() )
			return;

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
	[Rpc.Broadcast]
	public void AddTexture( Texture texture, int gradientWidth, float worldWidth, Vector2 position, Rotation2D rotation,
		int matCode )
	{
		if ( !AssertIsHost() )
			return;

		var matConfig = GetMaterialsFromCode( matCode );
		var activeMaterials = GetActiveMaterials( matConfig );

		var textureSdf = new TextureSdf( texture, gradientWidth, worldWidth );
		var transformedTextureSdf = textureSdf
			.Transform( position, rotation );
		foreach ( var (material, offset) in activeMaterials )
			Add( SdfWorld, transformedTextureSdf.Expand( offset ), material );
	}

	/// <summary>
	/// Creates the standard world box for generated terrain.
	/// </summary>
	/// <param name="length">The length of the world.</param>
	/// <param name="height">The height of the world.</param>
	/// <param name="fgMaterial">The material for the foreground of the world.</param>
	/// <param name="bgMaterial">The material for the background of the world.</param>
	[Rpc.Broadcast]
	private void AddWorldBox( int length, int height )
	{
		if ( !AssertIsHost() )
			return;

		_lengthOffset = length / 2;
		_heightOffset = 0;

		var matConfig = GetMaterialsFromCode( 0 );
		var activeMaterials = GetActiveMaterials( matConfig );
		var bgMat = RockMaterial;

		var boxSdf = new RectSdf( new Vector2( -length / 2f, 0 ), new Vector2( length / 2, height ) );
		Add( SdfWorld, boxSdf, activeMaterials.ElementAt( 0 ).Key );
		Add( SdfWorld, boxSdf, bgMat );
	}

	private bool AssertIsHost()
	{
		return Connection.Local == Connection.Host;
	}

	public TimeSince TimeSinceLastModification { get; set; }

	/// <summary>
	/// Wrapper for a standard Sdf addition.
	/// </summary>
	/// <param name="world">The world to subtract from.</param>
	/// <param name="sdf">The Sdf to apply.</param>
	/// <param name="material">The material to apply.</param>
	private void Add( Sdf2DWorld world, ISdf2D sdf, Sdf2DLayer material )
	{
		TimeSinceLastModification = 0f;
		sdf = sdf.Translate( new Vector2( 0, -WorldPosition.z ) );
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
		TimeSinceLastModification = 0f;
		sdf = sdf.Translate( new Vector2( 0, -WorldPosition.z ) );
		if ( offset )
			sdf = sdf.Translate( new Vector2( -_lengthOffset, _heightOffset ) );
		world.SubtractAsync( sdf, material );
	}
}
