using Sandbox.Sdf;
using Sandbox.Utility;

namespace Grubs.Terrain;

public partial class GrubsTerrain
{
	private const int RandomMax = 99999;
	
	private void CreateNoiseMap()
	{
		var resolution = 1;
		
		var worldLength = GrubsConfig.TerrainLength;
		var worldHeight = GrubsConfig.TerrainHeight;

		var pointsX = worldLength * resolution;
		var pointsY = worldHeight * resolution;

		var random = Game.Random.Int( RandomMax );
		var heightMap = new float[pointsX];
		var noiseMap = new float[pointsX, pointsY];

		var freq = 0.15f;

		Log.Info( $"NM L0: {noiseMap.GetLength( 0 )}, NM L1: {noiseMap.GetLength( 1 )}" );

		// Generate heightMap, which determines base terrain curve.
		for ( var x = 0; x < pointsX; x++ )
		{
			var noise = Noise.Perlin( (x + random) * freq );
			heightMap[x] = noise * worldHeight * TerrainCurve.Evaluate( (float)x / pointsX );
		}
		
		// After generating heightMap...
		for (var x = 0; x < pointsX; x++)
		{
			for (var y = 0; y < pointsY; y++)
			{
				// If current y position is below the height at this x, 
				// set to 0 (empty space), otherwise 1 (solid terrain)
				var dist = y - heightMap[x];
				noiseMap[x, y] = dist;
			}
		}
		
		var terrainSdf = new NoiseSdf2D(
			new Vector2( -worldLength / 2f, 0 ), 
			new Vector2( worldLength / 2f, worldHeight ), 
			noiseMap );
		
		var cfg = new MaterialsConfig( true, true );
		var materials = GetActiveMaterials( cfg );
		Add( SdfWorld, terrainSdf, materials.ElementAt( 0 ).Key );
	}
}
