namespace Grubs;

public partial class Terrain
{
	void SetupGeneratedWorld()
	{
		AddWorldBox( GrubsConfig.TerrainLength, GrubsConfig.TerrainHeight );
	}
}
