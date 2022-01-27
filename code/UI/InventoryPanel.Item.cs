using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using Grubs.Pawn;
using Grubs.Weapons;

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

				var inventory = (Local.Pawn as Pawn.Player).PlayerInventory;

				if ( inventory == null )
					return this;

				if ( inventory.Items[weaponIndex] is not Weapon weapon )
					return this;

				var ammoCount = weapon.Ammo;

				SlotIndex = weaponIndex;
				SetClass( "Occupied", true );
				SetClass( "Empty", ammoCount == 0 );

				var weaponName = weapon.ClassInfo.Name;
				Add.Image( $"{weapon.ModelPath}_c.png", "Icon" );

				if ( ammoCount >= 0 )
					Add.Label( $"{ammoCount}", "Ammo" );

				return this;
			}

			public void EquipItem()
			{
				if ( SlotIndex == -1 )
					return;

				PlayerInventory.EquipItemFromIndex( SlotIndex );
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
