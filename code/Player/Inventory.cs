namespace Grubs;

public partial class Inventory : EntityComponent<Player>
{
	[Net]
	public IList<Weapon> Weapons { get; private set; }

	[Net, Predicted]
	public Weapon ActiveWeapon { get; private set; }

	[Net, Predicted]
	public Weapon LastActiveWeapon { get; private set; }

	public void Add( Weapon weapon, bool makeActive = false )
	{
		if ( !weapon.IsValid() )
			return;

		if ( IsCarrying( weapon ) )
		{
			var existingWeapon = Weapons.FirstOrDefault(
				item => item.GetType() == weapon.GetType() );

			if ( existingWeapon is not null && existingWeapon.Ammo != -1 )
				existingWeapon.Ammo++;

			weapon.Delete();
			return;
		}

		Weapons.Add( weapon );

		if ( makeActive )
			SetActiveWeapon( weapon );
	}

	public void SetActiveWeapon( Weapon weapon )
	{
		if ( ActiveWeapon == weapon )
			return;

		if ( weapon.IsValid() && !weapon.HasAmmo() )
			return;

		if ( ActiveWeapon.IsValid() )
			ActiveWeapon.OnHolster();

		ActiveWeapon = weapon;
		ActiveWeapon?.OnDeploy( Entity.ActiveGrub );
	}

	public void UnsetActiveWeapon()
	{
		SetActiveWeapon( null );
	}

	public bool IsCarrying( Weapon weapon )
	{
		return Weapons.Any( item => item.Name == weapon.Name );
	}

	public bool HasAmmo( int index )
	{
		return Weapons[index].Ammo != 0;
	}

	public void Simulate( IClient client )
	{
		if ( Entity.ActiveWeaponInput is Weapon weapon )
		{
			SetActiveWeapon( weapon );
			Entity.ActiveWeaponInput = null;
		}

		ActiveWeapon?.Simulate( client );
	}

	[ConCmd.Admin( "gr_set_weapon" )]
	public static void SetWeapon( int weaponIndex )
	{
		if ( ConsoleSystem.Caller.Pawn is not Player player )
			return;

		if ( !player.IsTurn )
			return;

		var weapon = player.Inventory.Weapons[weaponIndex];
		player.Inventory.LastActiveWeapon = weapon;
		player.Inventory.SetActiveWeapon( weapon );
	}
}
