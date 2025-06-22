using Grubs.Systems.LevelEditing;
using Grubs.Systems.Pawn;

namespace Grubs.Systems.GameMode;

[Title( "Grubs - Level Editor Mode"), Category( "Grubs/LevelEditing" ), Icon( "edit" )]
public sealed class LevelEditor : BaseGameMode
{
	private static readonly Logger Log = new( "LevelEditor" );
	
	public override string Name => "Level Editor";
	protected override int MaxPlayers => 1;
	
	[Property]
	public GameObject LevelEditorPawnPrefab { get; set; }
	
	public LevelDefinition LevelDefinition { get; private set; }

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

	public void CreateNewLevelDefinition( string name )
	{
		LevelDefinition = new LevelDefinition
		{
			Id = Guid.NewGuid(),
			DisplayName = name
		};
	}

	public void LoadLevelDefinition( LevelDefinition definition )
	{
		LevelDefinition = definition;
	}
}
