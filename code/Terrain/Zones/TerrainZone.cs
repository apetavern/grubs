namespace Grubs;

/// <summary>
/// Handles a zone in the terrain that can trigger on entities.
/// </summary>
[Category( "Terrain" )]
public partial class TerrainZone : ModelEntity
{
	public override void Spawn()
	{
		Tags.Add( "trigger" );
	}

	/// <summary>
	/// Sets the position this zone sits at.
	/// </summary>
	/// <param name="position">The position for the zone to sit at.</param>
	/// <returns>The terrain zone instance.</returns>
	public TerrainZone WithPosition( Vector3 position )
	{
		Position = position;
		return this;
	}

	/// <summary>
	/// Sets the bbox that the zone will use.
	/// </summary>
	/// <param name="bbox">The bbox to use.</param>
	/// <returns>The terrain zone instance.</returns>
	public TerrainZone WithBBox( BBox bbox )
	{
		SetupPhysicsFromAABB( PhysicsMotionType.Static, bbox.Mins, bbox.Maxs );
		return this;
	}

	/// <summary>
	/// Returns the zone as the provided type.
	/// </summary>
	/// <returns>The terrain zone as the provided type.</returns>
	public virtual T Finish<T>() where T : TerrainZone
	{
		return (this as T)!;
	}

	/// <summary>
	/// Queues the zone to be added to the zone list.
	/// </summary>
	/// <returns>The terrain zone in the base type.</returns>
	public TerrainZone Finish()
	{
		return Finish<TerrainZone>();
	}

	/// <summary>
	/// Debug console variable to see the zones area.
	/// </summary>
	[ConVar.Server( "gr_zone_debug" )]
	public static bool ZoneDebug { get; set; }

	[Event.Tick.Server]
	public void Tick()
	{
		if ( !ZoneDebug )
			return;

		DebugOverlay.Box( Position, CollisionBounds.Mins, CollisionBounds.Maxs, Color.Yellow, 0, false );
	}
}
