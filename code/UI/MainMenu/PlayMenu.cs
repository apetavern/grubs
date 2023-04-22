namespace Grubs.UI;

public partial class PlayMenu : Panel
{
	[ConCmd.Server]
	public static void StartGame()
	{
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
