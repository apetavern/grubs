namespace Grubs;

public partial class GameStatePanel : Panel
{
	[ConCmd.Server]
	public static void StartGame()
	{
		if ( GamemodeSystem.Instance is not FreeForAll ffa )
			return;

		if ( Game.Clients.Count >= GrubsConfig.MinimumPlayers && ffa.TerrainReady && !ffa.Started )
			ffa.Start();
	}
}
