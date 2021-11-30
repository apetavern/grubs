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
				AddEventListener( "onclick", () => EquipItem() );
			}

			public InventoryItem UpdateFrom( int weaponIndex )
			{
				Reset();

				var inventory = Local.Pawn.Inventory as Inventory;
				if ( inventory == null ) return this;

				if ( inventory.GetSlot( weaponIndex ) is not Weapon weapon )
					return this;

				var ammoCount = (inventory.GetSlot( weaponIndex ) as Weapon).Ammo;

				SlotIndex = weaponIndex;
				SetClass( "Occupied", true );
				SetClass( "Empty", ammoCount == 0 );


				Add.Image( $"/materials/icons/{weapon.ClassInfo.Name}.png", "Icon" );

				if ( ammoCount >= 0 )
					Add.Label( $"{ammoCount}", "Ammo" );

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
