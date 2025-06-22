using Grubs.Systems.LevelEditing;
using Grubs.Systems.Pawn;
using Grubs.Terrain;
using Sandbox.Utility;

namespace Grubs.Systems.GameMode;

[Title( "Grubs - Level Editor Mode"), Category( "Grubs/LevelEditing" ), Icon( "edit" )]
public sealed class LevelEditor : BaseGameMode
{
	private static readonly Logger Log = new( "LevelEditor" );
	
	public override string Name => "Level Editor";
	protected override int MaxPlayers => 1;
	
	[Property]
	public GameObject LevelEditorPawnPrefab { get; set; }

	public LevelEditorState State { get; set; } = LevelEditorState.Menu;

	protected override void OnModeInit()
	{
		base.OnModeInit();
		
		Log.Info( "Initializing level editor game mode..." );
	}

	public void SpawnLevelEditorPawn()
	{
		if ( !LevelEditorPawnPrefab.IsValid() )
		{
			Log.Error( "No level editor prefab could be spawned." );
			Game.Close();
			return;
		}

		var editor = LevelEditorPawnPrefab.Clone();
		Log.Info( $"Editor pawn has spawned: {editor.Name}" );
	}

	public void CreateNewLevelDefinition( string name, string description )
	{
		var definition = new LevelDefinition
		{
			Id = Guid.NewGuid(),
			DisplayName = name,
			Description = description,
			Tags = [],
			CreatedBy = new Friend( Steam.SteamId ),
			CreatedOn = DateTime.UtcNow,
			LastUpdated = DateTime.UtcNow,
			TerrainSize = TerrainSize.Medium
		};

		_ = CreateNewLevelDefinitionAsync( definition );
	}

	private async Task CreateNewLevelDefinitionAsync( LevelDefinition definition )
	{
		await GameTerrain.Local.CreateDefinition( definition );
		
		SetLevelEditorState( LevelEditorState.Editing );
	}

	public void SetLevelEditorState( LevelEditorState state )
	{
		State = state;
	}
}

public enum LevelEditorState
{
	Menu,
	Editing
}
