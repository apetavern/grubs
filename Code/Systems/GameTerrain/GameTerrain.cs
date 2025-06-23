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

	protected override void OnStart()
	{
		base.OnStart();
		
		SdfWorld.WorldRotation = Rotation.FromRoll( 90f );
	}

	public async Task CreateDefinition( LevelDefinition definition )
	{
		LevelDefinition = definition;
		
		await WriteDefinitionToFile( definition );
		await SdfWorld.ClearAsync();
	}

	public async Task LoadDefinition( LevelDefinition definition )
	{
		LevelDefinition = definition;

		// OverrideLayerMaterial( GenericMaterial, definition.TerrainForegroundMaterial );

		if ( definition.TerrainScorchOverride != Color.Transparent )
		{
			// OverrideLayerScorchColor( GenericMaterial, definition.TerrainScorchOverride );
		}

		if ( LevelDefinition.Modifications is null || LevelDefinition.Modifications.Length == 0 )
		{
			return;
		}
		
		var byteStream = ByteStream.CreateReader( LevelDefinition.Modifications );
		SdfWorld.ClearAndReadData( ref byteStream );
		byteStream.Dispose();
	}
}
