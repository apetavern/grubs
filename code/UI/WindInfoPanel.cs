using Sandbox.UI;
using Sandbox.UI.Construct;

namespace TerryForm.UI
{
	public class WindInfoPanel : Panel
	{
		public WindInfoPanel()
		{
			StyleSheet.Load( "/Code/UI/WindInfoPanel.scss" );

			int sideSegmentCount = 9;

			// Left segments
			for ( int i = 0; i < sideSegmentCount; ++i )
			{
				Add.Icon( "arrow_left" );
			}

			// Right segments
			for ( int i = 0; i < sideSegmentCount; ++i )
			{
				Add.Icon( "arrow_right" );
			}
		}
	}
}
