namespace Grubs.Terrain.Shapes;

/// <summary>
/// Handles multiple zone shapes.
/// </summary>
public partial class MultiShape : ZoneShape
{
	[Net]
	private IList<ZoneShape> Shapes { get; set; }

	/// <summary>
	/// Adds a new shape.
	/// </summary>
	/// <param name="shape">The shape to add.</param>
	/// <returns>The multi shape instance.</returns>
	public MultiShape AddShape( ZoneShape shape )
	{
		shape.Zone = Zone;
		Shapes.Add( shape );
		return this;
	}

	public override bool InZone( Entity entity )
	{
		return Shapes.Any( shape => shape.InZone( entity ) );
	}

	public override void DebugDraw()
	{
		foreach ( var shape in Shapes )
			shape.DebugDraw();
	}
}
