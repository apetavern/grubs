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
}
