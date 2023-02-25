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

	[ConCmd.Server]
	public static void RestartGame()
	{
		Game.ResetMap( Array.Empty<Entity>() );

		GamemodeSystem.Instance.Delete();
		GamemodeSystem.SetupGamemode();
		GamemodeSystem.Instance.GameWorld = new World();

		World.RegenWorld();
	}
}
