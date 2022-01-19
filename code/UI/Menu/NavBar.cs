using Sandbox.UI;

namespace Grubs.UI.Menu
{
	[UseTemplate]
	public partial class NavBar : Panel
	{
		public NavBar()
		{
			StyleSheet.Load( "/UI/Menu/Navbar.scss" );
		}
	}
}
