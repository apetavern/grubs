using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using TerryForm.Weapons;
using TerryForm.Pawn;

namespace TerryForm.UI
{
	public partial class InventoryPanel
	{
		public class InventoryItem : Panel
		{
			private int SlotIndex { get; set; } = -1;

			public InventoryItem()
			{
				StyleSheet.Load( "/Code/UI/InventoryPanel.scss" );
				AddEventListener( "onclick", () => EquipItem( SlotIndex ) );
			}

			public InventoryItem UpdateFrom( int weaponIndex )
			{
				Reset();

				if ( Local.Pawn.Inventory.GetSlot( weaponIndex ) is not Weapon weapon )
					return this;

				SlotIndex = weaponIndex;
				SetClass( "Occupied", true );

				Add.Image( $"/materials/icons/{weapon.ClassInfo.Name}.png", "Icon" );

				return this;
			}

			public void EquipItem( int weaponIndex )
			{
				if ( SlotIndex == -1 ) return;

				Inventory.EquipItemFromIndex( weaponIndex );
			}

			private void Reset()
			{
				DeleteChildren();
				SetClass( "Occupied", false );
				SlotIndex = -1;
			}
		}
	}
}
