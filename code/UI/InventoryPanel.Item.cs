using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using Grubs.Player;
using Grubs.Weapons.Base;

namespace Grubs.UI
{
	public partial class InventoryPanel
	{
		public class InventoryItem : Panel
		{
			private int SlotIndex { get; set; } = -1;

			public InventoryItem()
			{
				StyleSheet.Load( "/UI/InventoryPanel.scss" );
				AddEventListener( "onclick", EquipItem );
			}

			public InventoryItem UpdateFrom( int weaponIndex )
			{
				Reset();

				var inventory = (Local.Pawn as Team)!.Inventory;

				if ( inventory == null )
					return this;

				if ( inventory.Items[weaponIndex] is not GrubWeapon weapon )
					return this;

				var ammoCount = weapon.Ammo;

				SlotIndex = weaponIndex;
				SetClass( "Occupied", true );
				SetClass( "Empty", ammoCount == 0 );

				var weaponName = weapon.WeaponName;
				Add.Image( $"{weapon.Icon}", "Icon" );

				if ( ammoCount >= 0 )
					Add.Label( $"{ammoCount}".Truncate(1), "Ammo" );

				return this;
			}

			public void EquipItem()
			{
				if ( SlotIndex == -1 )
					return;

				GrubsInventory.EquipItemByIndex(SlotIndex);
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
