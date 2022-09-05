using Grubs.Weapons.Base;

namespace Grubs.Player;

public partial class GrubsInventory : BaseNetworkable
{
	public Entity Owner { get; set; }
	[Net] public List<GrubWeapon> Items { get; set; } = new();

	public void Add( GrubWeapon weapon, bool makeActive = false )
	{
		if ( !weapon.IsValid() )
			return;

		// Handle picking up a weapon we already have.
		if ( IsCarrying( weapon ) )
		{
			var existingWeapon = Items.FirstOrDefault( item => item.GetType() == weapon.GetType() );
			// -1 represents unlimited ammo, so don't add ammo in this case.
			if ( existingWeapon is not null && existingWeapon.Ammo != -1 )
				existingWeapon.Ammo++;

			weapon.Delete();
			return;
		}

		// Handle picking up a weapon we do not have.
		Items.Add( weapon );
		weapon.Parent = Owner;
		weapon.OnCarryStart( Owner );
	}

	public bool IsCarrying( GrubWeapon weapon )
	{
		return Items.Any( item => item.Name == weapon.Name );
	}

	public bool HasAmmo( int index )
	{
		return Items[index].Ammo != 0;
	}

	[ConCmd.Server]
	public static void EquipItemByIndex( int index )
	{
		if ( ConsoleSystem.Caller.Pawn is not Team team )
			return;

		var grub = team.ActiveGrub;
		if ( !grub.IsTurn )
			return;

		var inventory = team.Inventory;
		grub.EquipWeapon( inventory.Items[index] );
	}
}
