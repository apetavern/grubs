using Sandbox.MarchingCubes;
using Sandbox.Sdf;

namespace Grubs;

public partial class Terrain
{
	void SetupWorldFromTexture()
	{
		DoTextureLoad();
	}

	async void DoTextureLoad()
	{
		var mapSdfTexture = await Texture.LoadAsync( FileSystem.Mounted, "textures/texturelevels/" + GrubsConfig.WorldTerrainTexture.ToString() + ".png" );
		var mapSdf = new TextureSdf( mapSdfTexture, 10, mapSdfTexture.Width * 2f );
		var transformedSdf = mapSdf.Transform( new Vector2( -GrubsConfig.TerrainLength / 2, 0 ) );

		var cfg = new MaterialsConfig( true, true );
		var materials = GetActiveMaterials( cfg );

		SdfWorld.Add( transformedSdf, materials.ElementAt( 0 ).Key );

		mapSdfTexture = await Texture.LoadAsync( FileSystem.Mounted, "textures/texturelevels/" + GrubsConfig.WorldTerrainTexture.ToString() + "_back.png" );
		mapSdf = new TextureSdf( mapSdfTexture, 10, mapSdfTexture.Width * 2f );
		transformedSdf = mapSdf.Transform( new Vector2( -GrubsConfig.TerrainLength / 2, 0 ) );

		SdfWorld.Add( transformedSdf, materials.ElementAt( 1 ).Key );

	}
}
