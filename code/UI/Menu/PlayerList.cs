using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Grubs.UI.Menu
{
	public partial class PlayerEntry : Panel
	{
		public Image Avatar { get; set; }
		public Label PlayerName { get; set; }

		public PlayerEntry()
		{
			Avatar = Add.Image( "", "avatar" );
			PlayerName = Add.Label( "Name", "name" );
		}
	}

	[UseTemplate]
	public partial class PlayerList : Panel
	{
		public PlayerList()
		{
			StyleSheet.Load( "/UI/Menu/PlayerList.scss" );
		}
	}
}
