namespace Grubs.UI;

public partial class PlayMenu : Panel
{
	[ConCmd.Server]
	public static void StartGame()
	{
		if ( Game.Clients.Count < GamemodeSystem.Instance.MinimumPlayers )
			return;

		if ( !GamemodeSystem.Instance.WorldReady || GamemodeSystem.Instance.CurrentState != Gamemode.State.MainMenu )
			return;

		GamemodeSystem.Instance.Start();
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
