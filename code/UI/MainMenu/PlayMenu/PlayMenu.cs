namespace Grubs.UI;

public partial class PlayMenu : Panel
{
	[ConCmd.Server]
	public static void StartGame()
	{
		GamemodeSystem.Instance.Start();
	}
}
