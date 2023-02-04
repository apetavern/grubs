namespace Grubs;

public partial class Inventory : EntityComponent<Player>
{
	[Net]
	public IList<Weapon> Weapons { get; private set; }

	[Net, Predicted]
	public Weapon ActiveWeapon { get; private set; }

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
		var currentWeapon = ActiveWeapon;
		if ( currentWeapon.IsValid() )
		{
			currentWeapon.OnHolster();

			ActiveWeapon = null;
		}

		ActiveWeapon = weapon;
		weapon?.OnDeploy( Entity.ActiveGrub );
	}

	public bool IsCarrying( Weapon weapon )
	{
		return Weapons.Any( item => item.Name == weapon.Name );
	}
}
