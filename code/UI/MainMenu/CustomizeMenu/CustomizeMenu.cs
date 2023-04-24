namespace Grubs.UI;

public partial class CustomizeMenu
{
	[ConCmd.Server]
	public static void SetColor( Color color )
	{
		if ( ConsoleSystem.Caller.Pawn is not Player player )
			return;

		player.ActiveColor = color;
	}
}
