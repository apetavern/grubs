using Sandbox.UI;
using Sandbox.UI.Construct;

namespace TerryForm.UI
{
	public class WindInfoPanel : Panel
	{
		public WindInfoPanel()
		{
			StyleSheet.Load( "/Code/UI/WindInfoPanel.scss" );

			int segmentCount = 9;
			for ( int i = 0; i < segmentCount; ++i )
			{
				Add.Icon( "arrow_left" );
			}
			for ( int i = 0; i < segmentCount; ++i )
			{
				Add.Icon( "arrow_right" );
			}
		}
	}
}
