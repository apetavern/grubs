using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System.Collections.Generic;
using TerryForm.Weapons;

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
				icon.AddEventListener( "onclick", _ => Log.Info( "clicked" ) );

				Items.Add( i, icon );
			}

			BindClass( "open", () => Input.Down( InputButton.Menu ) );
		}

		public void RebuildItems()
		{
			for ( int i = 0; i < Local.Pawn.Inventory.Count(); i++ )
			{
				var entity = Local.Pawn.Inventory.GetSlot( i );
				Items[i].UpdateFrom( entity as Weapon );
			}

			HasBuilt = true;
		}

		public override void Tick()
		{
			base.Tick();

			if ( Input.Down( InputButton.Menu ) && !HasBuilt )
				RebuildItems();

			if ( Input.Released( InputButton.Menu ) )
				HasBuilt = false;
		}
	}
}
