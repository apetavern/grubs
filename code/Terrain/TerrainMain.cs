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

	{
		{
		}

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
}
