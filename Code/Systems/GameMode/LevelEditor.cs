using Grubs.Systems.LevelEditing;
using Grubs.Systems.Pawn;
using Grubs.Terrain;
using Sandbox.Sdf;
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
	
	[Property]
	public Material DefaultLevelForegroundMaterial { get; set; }

	public LevelEditorState State { get; set; } = LevelEditorState.Menu;

	public List<LevelDefinition> LevelDefinitions { get; set; } = [];

	protected override void OnModeInit()
	{
		base.OnModeInit();
		
		Log.Info( "Initializing level editor game mode..." );

		_ = LoadLevelDefinitions();
	}

	private async Task LoadLevelDefinitions()
	{
		const string levelDataDirectory = "levels";

		if ( !FileSystem.Data.DirectoryExists( levelDataDirectory ) )
			return;

		var files = FileSystem.Data.FindFile( levelDataDirectory ).ToList();
		Log.Info( $"Loaded {files.Count} level definitions" );
		
		foreach ( var file in files )
		{
			var data = await FileSystem.Data.ReadAllTextAsync( $"levels/{file}" );
			var levelData = JsonSerializer.Deserialize<LevelDefinitionData>( data );

			foreach ( var layerDefinition in levelData.Layers )
			{
				var layer = layerDefinition.GetLayer();
				// hack ?!??!?!
				layer.ReferencedTextures
					.First().Source = ResourceLibrary.Get<Sdf2DLayer>( "materials/sdf/scorch.sdflayer" );
				
				Log.Info( $"Adding {layer.DynamicId} to LayerUtility." );
				LayerUtility.AddLayer( layer.DynamicId, layer );
			}
			
			LevelDefinitions.Add( levelData.ToDefinition() );
		}
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
		Log.Info( $"Creating new level definition with name {name}" );
		
		var layerDefinition = new LayerDefinition( "",
			DefaultLevelForegroundMaterial.ResourcePath, DefaultLevelForegroundMaterial.ShaderName );
		
		var definition = new LevelDefinition
		{
			Id = Guid.NewGuid(),
			DisplayName = name,
			Description = description,
			Tags = [],
			CreatedBy = new Friend( Steam.SteamId ),
			CreatedOn = DateTime.UtcNow,
			LastUpdated = DateTime.UtcNow,
			TerrainSize = TerrainSize.Medium,
			Layers = [layerDefinition],
			TerrainScorchOverride = Color.Transparent
		};

		_ = CreateNewLevelDefinitionAsync( definition );
	}

	private async Task CreateNewLevelDefinitionAsync( LevelDefinition definition )
	{
		await GameTerrain.Local.CreateDefinition( definition );
		
		SetLevelEditorState( LevelEditorState.Editing );
	}

	public void LoadExistingLevelDefinition( LevelDefinition definition )
	{
		Log.Info( $"Loading existing level definition {definition.DisplayName}" );
		
		_ = LoadExistingLevelDefinitionAsync( definition );
	}

	private async Task LoadExistingLevelDefinitionAsync( LevelDefinition definition )
	{
		await GameTerrain.Local.LoadDefinition( definition );
		
		SetLevelEditorState( LevelEditorState.Editing );
	}
	
	public void SetLevelEditorState( LevelEditorState state )
	{
		Log.Info( $"Setting level editor state to {state}" );
		State = state;

		if ( State is LevelEditorState.Editing )
		{
			SpawnLevelEditorPawn();
		}
		else
		{
			Log.Info( "Going back to menu. Cleaning up!" );
			var editor = Scene.GetAllComponents<EditorPlayer>().FirstOrDefault();
			if ( editor.IsValid() )
			{
				editor.GameObject.Destroy();
			}
		}
	}
}

public enum LevelEditorState
{
	Menu,
	Editing
}
