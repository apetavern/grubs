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

			Add.Label( "Inventory", "title" );

			var itemsPanel = Add.Panel( "inventory-items" );

			const int rows = 6;
			const int cols = 5;

			for ( int i = 0; i < rows * cols; ++i )
			{
				itemsPanel.AddChild<InventoryItem>();
			}

			BindClass( "open", () => Input.Down( InputButton.Menu ) );
		}
	}
}
