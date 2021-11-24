using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace TerryForm.UI
{
	public class WindInfoPanel : Panel
	{
		public WindInfoPanel()
		{
			StyleSheet.Load( "/Code/UI/WindInfoPanel.scss" );

			Add.Panel( "wind-inner" );
		}
	}
}
