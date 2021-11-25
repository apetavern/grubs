using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using TerryForm.UI.Elements;

namespace TerryForm.UI
{
	[UseTemplate]
	public class PlayerInfoPanel : Panel
	{
		public PlayerInfoPanel()
		{
			StyleSheet.Load( "/Code/UI/PlayerInfoPanel.scss" );
		}
	}
}
