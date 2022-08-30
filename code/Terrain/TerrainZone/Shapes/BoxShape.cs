namespace Grubs.Terrain.Shapes;

/// <summary>
/// A box shape.
/// </summary>
public partial class BoxShape : ZoneShape
{
	/// <summary>
	/// The size of the box.
	/// </summary>
	[Net]
	public Vector3 Size { get; set; }

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
		return new BoxShape { Size = size };
	}
}
