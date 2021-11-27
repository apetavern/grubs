using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace TerryForm.UI
{
	public partial class InventoryPanel
	{
		public class InventoryItem : Panel
		{
			public InventoryItem()
			{
				StyleSheet.Load( "/Code/UI/InventoryPanel.scss" );
			}
		}
	}
}
