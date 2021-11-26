using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace TerryForm.UI
{
	public partial class InventoryPanel : Panel
	{
		public InventoryPanel()
		{
			StyleSheet.Load( "/Code/UI/InventoryPanel.scss" );

			for ( int i = 0; i < 24; ++i )
			{
				AddChild<InventoryItem>();
			}
		}
	}
}
