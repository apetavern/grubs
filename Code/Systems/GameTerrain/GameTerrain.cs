using Grubs.Common;
using Grubs.Systems.LevelEditing;
using Sandbox.Sdf;

namespace Grubs.Terrain;

[Title( "Grubs - Game Terrain" ), Category( "Grubs" ), Icon( "landscape" )]
public partial class GameTerrain : LocalComponent<GameTerrain>
{
	public LevelDefinition LevelDefinition { get; private set; }

	[Property]
	public Sdf2DWorld SdfWorld { get; private set; }
}
