using Sandbox.UI;

namespace Grubs.UI.Menu
{
	[UseTemplate]
	public partial class StartButton : Panel
	{
		public StartButton()
		{
			StyleSheet.Load( "/UI/Menu/StartButton.scss" );
		}
	}
}
