using Grubs.Systems.Pawn;
using Grubs.Terrain;

namespace Grubs.Systems.GameMode;

[Title( "Free For All" ), Category( "Grubs/Game Mode" )]
public sealed class FreeForAll : BaseGameMode
{
	public override string Name => "Free For All";

	protected override void OnModeInit()
	{
		Log.Info( $"{Name} mode initializing." );

		GrubsTerrain.Instance.Init();
	}

	protected override void OnModeStarted()
	{
		Log.Info( $"{Name} mode starting." );
	}

	protected override void OnPlayerJoined( Player player )
	{
		Log.Info( $"Adding {player.GameObject.Name} to Free For All game mode." );
	}
}
