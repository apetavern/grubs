using Sandbox.UI;

namespace Grubs.UI.Menu
{
	[UseTemplate]
	public partial class MenuPanel : Panel
	{

		public MenuPanel()
		{
			StyleSheet.Load( "/UI/Menu/MenuPanel.scss" );
		}
	}
}
