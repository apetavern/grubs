namespace Grubs;

public partial class Inventory : EntityComponent<Player>
{
	[Net]
	public IList<Weapon> Weapons { get; private set; }

	[Net, Predicted]
	public Weapon ActiveWeapon { get; private set; }

	[Net, Predicted]
	public Weapon LastActiveWeapon { get; private set; }

	private Weapon _lastSwappedWeapon { get; set; }

	public void Simulate( IClient client )
	{
		if ( Entity.ActiveWeaponInput is Weapon weapon )
		{
			SetActiveWeapon( weapon );
			Entity.ActiveWeaponInput = null;
		}

		if ( _lastSwappedWeapon != ActiveWeapon )
		{
			LastActiveWeapon?.Holster( Entity.ActiveGrub );
			ActiveWeapon?.Deploy( Entity.ActiveGrub );

			_lastSwappedWeapon = ActiveWeapon;
		}

		ActiveWeapon?.Simulate( client );
	}

	public void Add( Weapon weapon, bool makeActive = false )
	{
		if ( !weapon.IsValid() )
			return;

		if ( IsCarrying( weapon ) )
		{
			var existingWeapon = Weapons.FirstOrDefault(
				item => item.Name == weapon.Name );

			if ( existingWeapon is not null && existingWeapon.Ammo != -1 )
				existingWeapon.Ammo++;

			weapon.Delete();
			return;
		}

		Weapons.Add( weapon );

		if ( makeActive )
			SetActiveWeapon( weapon );
	}

	public void SetActiveWeapon( Weapon weapon, bool forced = false )
	{
		if ( ActiveWeapon == weapon )
			return;

		if ( weapon.IsValid() && !weapon.HasAmmo() )
			return;

		if ( ActiveWeapon.IsValid() && ActiveWeapon.HasFired && !forced )
			return;

		LastActiveWeapon = ActiveWeapon;
		ActiveWeapon = weapon;
	}

	public bool IsCarrying( Weapon weapon )
	{
		return Weapons.Any( item => item.Name == weapon.Name );
	}

	public bool HasAmmo( int index )
	{
		return Weapons[index].Ammo != 0;
	}

	public void Clear()
	{
		Weapons.Clear();
		ActiveWeapon = null;
	}

	public void GiveDefaultLoadout()
	{
		foreach ( var prefab in Weapon.GetAllWeaponPrefabs() )
		{
			Assert.True( PrefabLibrary.TrySpawn<Weapon>( prefab.ResourcePath, out var weapon ) );
			Add( weapon );
		}
	}

	[ConCmd.Admin( "gr_set_weapon" )]
	public static void SetWeapon( int weaponIndex )
	{
		if ( ConsoleSystem.Caller.Pawn is not Player player )
			return;

		if ( !player.IsTurn )
			return;

		var weapon = player.Inventory.Weapons[weaponIndex];
		player.Inventory.SetActiveWeapon( weapon );
	}

	[ConCmd.Admin( "gr_unlimited_ammo" )]
	public static void SetUnlimitedAmmo()
	{
		if ( ConsoleSystem.Caller.Pawn is not Player player )
			return;

		foreach ( var weapon in player.Inventory.Weapons )
		{
			weapon.Ammo = -1;
			weapon.Charges = int.MaxValue;
		}
	}
}
