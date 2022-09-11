using Grubs.Player;
using Grubs.Utils.Event;
using Grubs.Weapons.Base;

namespace Grubs.UI;

public class InventoryPanel : Panel
{
	private Panel ItemsPanel { get; set; }
	private bool HasBuilt { get; set; }

	private readonly Dictionary<int, InventoryItem> _items = new();

	public InventoryPanel()
	{
		StyleSheet.Load( "/UI/InventoryPanel.scss" );

		Add.Label( "Inventory", "title" );

		ItemsPanel = Add.Panel( "inventory-items" );

		const int rows = 6;
		const int cols = 5;

		for ( var i = 0; i < rows * cols; i++ )
		{
			var icon = ItemsPanel.AddChild<InventoryItem>();
			_items.Add( i, icon );
		}

		BindClass( "open", () => Input.Down( InputButton.Menu ) );
	}

	private void RebuildItems()
	{
		for ( var i = 0; i < (Local.Pawn as Team)!.Inventory.Items.Count; i++ )
			_items[i].UpdateFrom( i );

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
