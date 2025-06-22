using Grubs.Terrain;
using Sandbox.Sdf;

namespace Grubs.Terrain;

public partial class GrubsTerrain
{
	void SetupWorldFromTexture()
	{
		DoTextureLoad();
	}

	async void DoTextureLoad()
	{
		var LastTerrainGenerated = GrubsConfig.WorldTerrainTexture;
		var mapSdfTexture = await Texture.LoadAsync( FileSystem.Mounted, "textures/texturelevels/" + GrubsConfig.WorldTerrainTexture + ".png" );
		WorldTextureHeight = mapSdfTexture.Height * 2;
		WorldTextureLength = mapSdfTexture.Width * 2;

		GrubsConfig.TerrainLength = mapSdfTexture.Width * 2;
		GrubsConfig.TerrainHeight = mapSdfTexture.Height * 2;

		var mapSdf = new TextureSdf( mapSdfTexture, 10, mapSdfTexture.Width * 2f, pivot: 0f );
		var transformedSdf = mapSdf.Transform( new Vector2( -GrubsConfig.TerrainLength / 2f, -64f ) );

		var cfg = new MaterialsConfig( true, true );
		var materials = GetActiveMaterials( cfg );

		await SdfWorld.AddAsync( transformedSdf, materials.ElementAt( 0 ).Key );

		mapSdfTexture = await Texture.LoadAsync( FileSystem.Mounted, "textures/texturelevels/" + GrubsConfig.WorldTerrainTexture + "_back.png" );
		mapSdf = new TextureSdf( mapSdfTexture, 10, mapSdfTexture.Width * 2f, pivot: 0f );
		transformedSdf = mapSdf.Transform( new Vector2( -GrubsConfig.TerrainLength / 2f, -64f ) );

		await SdfWorld.AddAsync( transformedSdf, materials.ElementAt( 1 ).Key );

		while ( LastTerrainGenerated == GrubsConfig.WorldTerrainTexture )
		{
			await Task.DelaySeconds( 3f );
		}

		RegenerateTextureTerrain();
	}

	[ConCmd( "gr_reload_texture_terrain" )]
	public static void RegenerateTextureTerrain()
	{
		if ( !Game.IsEditor && !Networking.IsHost )
			return;

		Instance.ResetTextureTerrain();
	}

	private async void ResetTextureTerrain()
	{
		await SdfWorld?.ClearAsync();
		WorldTextureLength = 0;
		WorldTextureHeight = 0;
		SetupWorldFromTexture();
	}
}
