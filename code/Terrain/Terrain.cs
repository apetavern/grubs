namespace Grubs.Terrain;

public class Terrain
{
	public static List<TerrainModel> TerrainModels { get; private set; } = null!;

	public static void Initialize()
	{
		TerrainModels = new();

		foreach ( var chunk in GrubsGame.Current.TerrainMap.TerrainGridChunks )
		{
			var terrainModel = new TerrainModel( chunk );
			TerrainModels.Add( terrainModel );
		}
	}

	public static void RefreshDirtyChunks()
	{
		var chunks = GrubsGame.Current.TerrainMap.TerrainGridChunks;
		for ( var i = 0; i < chunks.Count; i++ )
		{
			var chunk = chunks[i];
			if ( chunk.IsDirty )
			{
				TerrainModels[i].DestroyMeshAndCollision();
				TerrainModels[i].Delete();
				TerrainModels[i] = new TerrainModel( chunk );
				chunk.IsDirty = false;
			}
		}
	}
}
