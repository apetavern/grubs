using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using TerryForm.Weapons;

namespace TerryForm.UI
{
	public partial class InventoryPanel
	{
		public class InventoryItem : Panel
		{
			public InventoryItem()
			{
				StyleSheet.Load( "/Code/UI/InventoryPanel.scss" );
				AddEventListener( "onclick", () => Log.Info( "clicked" ) );
			}

			public InventoryItem UpdateFrom( int weaponIndex )
			{
				DeleteChildren();

				if ( Local.Pawn.Inventory.GetSlot( weaponIndex ) is not Weapon weapon )
					return this;

				SetClass( "Occupied", true );

				Add.Image( $"/materials/icons/{weapon.ClassInfo.Name}.png", "Icon" );

				return this;
			}
		}
	}
}
