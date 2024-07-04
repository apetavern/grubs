namespace Grubs.Equipment.Weapons;

[Title( "Grubs - Ninja Rope Weapon" ), Category( "Equipment" )]
public class NinjaRopeWeapon : Weapon
{
	public static float Timeout = 3f;

	protected override void HandleComplexFiringInput()
	{
		if ( Equipment.Grub is not { } grub )
			return;

		if ( Input.Pressed( "fire" ) && !IsFiring )
		{
			IsFiring = true;
			TimeSinceLastUsed = 0f;
			OnFire.Invoke( 100 );

			if ( WeaponInfoPanel is not null )
			{
				WeaponInfoPanel.Inputs = new Dictionary<string, string>()
				{
					{ "fire", "Release Hook" }
				};
			}
		}
		else if ( Input.Pressed( "fire" ) && IsFiring )
		{
			if ( grub.ActiveMountable is null )
			{
				Log.Warning( "Trying to unmount, but ActiveMountable is null?" );
				return;
			}

			grub.ActiveMountable.Dismount();
			FireFinished();
		}
	}

	public override void OnHolster()
	{
		// Player has fired a hook, but not released it.
		if ( IsFiring && TimesUsed == 0 )
			Equipment.UseAmmo();

		base.OnHolster();
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		// Use a shot if we missed and it's been a few seconds.
		if ( IsFiring && Equipment.Grub?.ActiveMountable is null && TimeSinceLastUsed > Timeout )
			FireFinished();
	}

	protected override void FireFinished()
	{
		if ( WeaponInfoPanel is not null )
		{
			WeaponInfoPanel.Inputs = new Dictionary<string, string>()
				{
					{ "fire", "Fire Hook" }
				};
		}

		base.FireFinished();
	}
}
