namespace Grubs.Terrain.Shapes;

/// <summary>
/// A box shape.
/// </summary>
public sealed partial class BoxShape : ZoneShape
{
	/// <summary>
	/// The size of the box.
	/// </summary>
	[Net]
	private Vector3 Size { get; set; } = Vector3.One;

	public override bool InZone( Entity entity )
	{
		var mins = Vector3.Min( Position, Position + Size );
		var maxs = Vector3.Max( Position, Position + Size );

		var position = entity.Position;
		return position.x >= mins.x && position.x <= maxs.x &&
			   position.y >= mins.y && position.y <= maxs.y &&
			   position.z >= mins.z && position.z <= maxs.z;
	}

	public override void DebugDraw()
	{
		DebugOverlay.Box( Position, Position + Size, Color.Yellow, 1 );
	}

	/// <summary>
	/// Returns a new box shape with the provided size.
	/// </summary>
	/// <param name="size">The size to make the box.</param>
	/// <returns>The new box shape.</returns>
	public static BoxShape WithSize( Vector3 size )
	{
		Assert.True( size.x > 0, $"{nameof( size )} must have an X value greater than 0" );
		Assert.True( size.y > 0, $"{nameof( size )} must have an Y value greater than 0" );
		Assert.True( size.z > 0, $"{nameof( size )} must have an Z value greater than 0" );

		return new BoxShape { Size = size };
	}
}
