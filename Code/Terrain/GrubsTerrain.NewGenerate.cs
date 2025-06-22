using Sandbox.Sdf;
using Sandbox.Utility;

namespace Grubs.Terrain;

public partial class GrubsTerrain
{
	private const int RandomMax = 99999;
	
	private void CreateNoiseMap()
	{
		var worldLength = GrubsConfig.TerrainLength;
		var worldHeight = GrubsConfig.TerrainHeight;

		var random = Game.Random.Int( RandomMax );

		var freq = GrubsConfig.TerrainFrequency;

		var heightMapSdf = new HeightmapSdf2D( 
			new Vector2( -worldLength / 2f, 0 ),
			new Vector2( worldLength / 2f, worldHeight ),
			freq,
			random );
		
		var noiseSdf = new NoiseSdf2D(
			new Vector2( -worldLength / 2f, 0 ),
			new Vector2( worldLength / 2f, worldHeight ),
			GrubsConfig.TerrainFrequency / 4f,
			GrubsConfig.TerrainNoiseZoom * 4f,
			random );
		
		var cfg = new MaterialsConfig( true, true );
		var materials = GetActiveMaterials( cfg );
		Add( SdfWorld, heightMapSdf, materials.ElementAt( 0 ).Key );
		Add( SdfWorld, heightMapSdf, RockMaterial );
		Subtract( SdfWorld, noiseSdf, materials.ElementAt( 0 ).Key );
	}
}
