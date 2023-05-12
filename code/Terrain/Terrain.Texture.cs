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

		SdfWorld.Add( mapSdf, SandMaterial );

		mapSdfTexture = await Texture.LoadAsync( FileSystem.Mounted, "textures/texturelevels/" + GrubsConfig.WorldTerrainTexture.ToString() + "_back.png" );
		mapSdf = new TextureSdf( mapSdfTexture, 10, mapSdfTexture.Width * 2f );

		SdfWorld.Add( mapSdf, RockMaterial );

	}
}
