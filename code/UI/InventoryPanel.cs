using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System.Collections.Generic;

namespace TerryForm.UI
{
	public partial class InventoryPanel : Panel
	{
		private Panel ItemsPanel { get; set; }
		private bool HasBuilt { get; set; }
		private Dictionary<int, InventoryItem> Items = new();

		public InventoryPanel()
		{
			StyleSheet.Load( "/Code/UI/InventoryPanel.scss" );

			Add.Label( "Inventory", "title" );

			ItemsPanel = Add.Panel( "inventory-items" );

			const int rows = 6;
			const int cols = 5;

			for ( int i = 0; i < rows * cols; ++i )
			{
				var icon = ItemsPanel.AddChild<InventoryItem>();
				Items.Add( i, icon );
			}

			BindClass( "open", () => Input.Down( InputButton.Menu ) );
		}

		public void RebuildItems()
		{
			for ( int i = 0; i < Local.Pawn.Inventory.Count(); i++ )
			{
				Items[i].UpdateFrom( i );
			}

			HasBuilt = true;
		}

		public override void Tick()
		{
			base.Tick();

			if ( Input.Down( InputButton.Menu ) && !HasBuilt )
			{
				RebuildItems();
				HudEntity.Instance?.ReceiveInput( true );
			}

			if ( Input.Released( InputButton.Menu ) )
			{
				HasBuilt = false;
				HudEntity.Instance?.ReceiveInput( false );
			}
		}
	}
}
