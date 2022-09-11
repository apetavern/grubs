using Grubs.Player;

namespace Grubs.UI;

public class InventoryItem : Panel
{
	private int SlotIndex { get; set; } = -1;

	public InventoryItem()
	{
		StyleSheet.Load( "/UI/Stylesheets/InventoryPanel.scss" );
		AddEventListener( "onclick", EquipItem );
	}

	public InventoryItem UpdateFrom( int weaponIndex )
	{
		Reset();

		if ( Local.Pawn is not Team team )
			return this;

		var inventory = team.Inventory;
		if ( inventory.Items[weaponIndex] is not { } weapon )
			return this;

		var ammoCount = weapon.Ammo;

		SlotIndex = weaponIndex;
		SetClass( "Occupied", true );
		SetClass( "Empty", ammoCount == 0 );

		Add.Image( $"{weapon.Icon}", "Icon" );
		if ( ammoCount >= 0 )
			Add.Label( $"{ammoCount}".Truncate( 1 ), "Ammo" );

		return this;
	}

	private void EquipItem()
	{
		if ( SlotIndex == -1 )
			return;

		GrubsInventory.EquipItemByIndex( SlotIndex );
	}

	private void Reset()
	{
		DeleteChildren();
		SetClass( "Occupied", false );
		SlotIndex = -1;
	}
}
