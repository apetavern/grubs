namespace Grubs.Terrain;

/// <summary>
/// Handles a shape that encompasses a zone.
/// </summary>
public abstract partial class ZoneShape : BaseNetworkable
{
	/// <summary>
	/// The zone that this shape is a part of.
	/// </summary>
	[Net]
	protected TerrainZone Zone { get; set; } = null!;

	/// <summary>
	/// The offset this shape has from the position of the zone.
	/// </summary>
	[Net]
	protected Vector3 Offset { get; set; }

	/// <summary>
	/// The position of the zone shape.
	/// </summary>
	protected Vector3 Position => Zone.Position + Offset;

	/// <summary>
	/// Sets the offset the shape has from the zone.
	/// </summary>
	/// <param name="offset">The offset from the zone.</param>
	/// <returns>The zone shape instance.</returns>
	public ZoneShape WithOffset( Vector3 offset )
	{
		Offset = offset;
		return this;
	}

	/// <summary>
	/// Returns whether the entity is inside this zone.
	/// </summary>
	/// <param name="entity">The entity to check.</param>
	/// <returns>Whether or not the entity is inside the zone.</returns>
	public abstract bool InZone( Entity entity );

	/// <summary>
	/// Called when the zone this shape is a part of has finished configuring.
	/// </summary>
	public virtual void Finish( TerrainZone zone )
	{
		Zone = zone;
	}

	/// <summary>
	/// Debug method to show the zone.
	/// </summary>
	public virtual void DebugDraw()
	{
	}
}
