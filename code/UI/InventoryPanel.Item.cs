using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using TerryForm.Pawn;
using TerryForm.Weapons;

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
				AddEventListener( "onclick", EquipItem );
			}

			public InventoryItem UpdateFrom( int weaponIndex )
			{
				Reset();

				var inventory = Local.Pawn.Inventory as Inventory;
				if ( inventory == null ) return this;

				if ( inventory.GetSlot( weaponIndex ) is not Weapon weapon )
					return this;

				var hasAmmo = inventory.HasAmmo( weaponIndex );

				SlotIndex = weaponIndex;
				SetClass( "Occupied", true );
				SetClass( "Empty", !hasAmmo );

				Add.Image( $"/materials/icons/{weapon.ClassInfo.Name}.png", "Icon" );

				return this;
			}

			public void EquipItem()
			{
				if ( SlotIndex == -1 ) return;

				Inventory.EquipItemFromIndex( SlotIndex );
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
