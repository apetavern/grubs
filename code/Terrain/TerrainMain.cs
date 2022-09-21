namespace Grubs.Terrain;

/// <summary>
/// The main class for terrains.
/// </summary>
public static partial class TerrainMain
{
	/// <summary>
	/// The <see cref="TerrainMap"/> in the world.
	/// </summary>
	public static TerrainMap Current { get; set; } = null!;

	private static List<TerrainModel> TerrainModels { get; set; } = null!;

	/// <summary>
	/// Initializes the terrain models.
	/// </summary>
	public static void Initialize()
	{
		TerrainModels = new List<TerrainModel>();

		foreach ( var chunk in Current.TerrainGridChunks )
		{
			var terrainModel = new TerrainModel( chunk );
			TerrainModels.Add( terrainModel );
		}
	}

	/// <summary>
	/// Refreshes any chunks that have changed and need re-making.
	/// </summary>
	public static void RefreshDirtyChunks()
	{
		var chunks = Current.TerrainGridChunks;
		for ( var i = 0; i < chunks.Count; i++ )
		{
			var chunk = chunks[i];
			if ( !chunk.IsDirty )
				continue;

			TerrainModels[i].DestroyMeshAndCollision();
			TerrainModels[i].Delete();
			TerrainModels[i] = new TerrainModel( chunk );
			chunk.IsDirty = false;
		}
	}

	/// <summary>
	/// The client receiver to explode a portion of the terrain map.
	/// </summary>
	/// <param name="midpoint">The center point of the explosion.</param>
	/// <param name="size">How big the explosion was.</param>
	[ClientRpc]
	public static void ExplodeClient( Vector2 midpoint, float size )
	{
		if ( Current.DestructSphere( midpoint, size ) )
			RefreshDirtyChunks();
	}

	/// <summary>
	/// Destruct a sphere in the <see cref="TerrainMap"/>.
	/// </summary>
	/// <param name="startpoint">The start point of the line to be destructed.</param>
	/// <param name="endPoint">The endpoint of the line to be destructed.</param>
	/// <param name="width">The size (radius) of the sphere to be destructed.</param>
	[ClientRpc]
	public static void LineClient( Vector3 startpoint, Vector3 endPoint, float width )
	{
		if ( Current.DestructLine( startpoint, endPoint, width ) )
			RefreshDirtyChunks();
	}
}
