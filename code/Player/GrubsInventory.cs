using Grubs.Weapons;

namespace Grubs.Player;

public partial class GrubsInventory : BaseNetworkable
{
	public Entity Owner { get; set; }
	[Net] public List<GrubWeapon> Items { get; set; } = new();

	public void Add( GrubWeapon weapon, bool makeActive = false )
	{
		if ( weapon is null || !weapon.IsValid() )
			return;

		// Handle picking up a weapon we already have.
		if ( IsCarryingType( weapon.GetType() ) )
		{
			var existingWeapon = Items.Where( item => item.GetType() == weapon.GetType() ).FirstOrDefault();
			// -1 represents unlimited ammo, so don't add ammo in this case.
			if ( existingWeapon.Ammo != -1 )
				existingWeapon.Ammo++;

			weapon.Delete();
			return;
		}

		// Handle picking up a weapon we do not have.
		Items.Add( weapon );
		weapon.Parent = Owner;
		weapon.OnCarryStart( Owner );
	}

	[ConCmd.Server]
	public static void EquipItemByIndex( int index )
	{
		var team = ConsoleSystem.Caller.Pawn as Team;
		var grub = team.ActiveGrub;

		if ( grub is null || !grub.IsTurn )
			return;

		var inventory = team.Inventory;

		if ( inventory.Items[index] is null )
			return;

		grub.EquipWeapon( inventory.Items[index] );
	}

	public bool IsCarryingType( Type type )
	{
		return Items.Any( item => item.GetType() == type );
	}

	public bool HasAmmo( int index )
	{
		return Items[index].Ammo != 0;
	}
}
