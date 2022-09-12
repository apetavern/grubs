namespace Grubs.Terrain.Shapes;

/// <summary>
/// A sphere shape.
/// </summary>
public sealed partial class SphereShape : ZoneShape
{
	/// <summary>
	/// The radius of the sphere.
	/// </summary>
	[Net]
	public float Radius { get; set; }

	public override bool InZone( Entity entity )
	{
		var zonePos = Position;
		var entityPos = entity.Position;

		return Math.Pow( entityPos.x - zonePos.x, 2 ) +
			Math.Pow( entityPos.y - zonePos.y, 2 ) +
			Math.Pow( entityPos.z - zonePos.z, 2 ) < Math.Pow( Radius, 2 );
	}

	public override void DebugDraw()
	{
		DebugOverlay.Sphere( Position, Radius, Color.Yellow, 1 );
	}

	/// <summary>
	/// Returns a new sphere shape with the provided radius.
	/// </summary>
	/// <param name="radius">The radius to make the sphere.</param>
	/// <returns>The new sphere shape.</returns>
	public static SphereShape WithRadius( float radius )
	{
		Assert.True( radius > 0, $"{nameof( radius )} must have a value greater than 0" );

		return new SphereShape { Radius = radius };
	}
}
