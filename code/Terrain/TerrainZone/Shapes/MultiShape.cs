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
		Shapes.Add( shape );
		return this;
	}

	public override bool InZone( Entity entity )
	{
		return Shapes.Any( shape => shape.InZone( entity ) );
	}

	public override void Finish( TerrainZone zone )
	{
		base.Finish( zone );

		foreach ( var shape in Shapes )
			shape.Finish( zone );
	}

	public override void DebugDraw()
	{
		foreach ( var shape in Shapes )
			shape.DebugDraw();
	}
}
