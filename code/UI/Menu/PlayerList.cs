using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Grubs.UI.Menu
{
	[UseTemplate]
	public partial class PlayerList : Panel
	{
		public Label playerName;

		public PlayerList()
		{
			StyleSheet.Load( "/UI/Menu/PlayerList.scss" );
		}
	}
}
