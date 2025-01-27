namespace Grubs.Equipment.Weapons;

[Title( "Grubs - Ninja Rope Weapon" ), Category( "Equipment" )]
public class NinjaRopeWeapon : Weapon
{
	[Property] private GameObject ProjectilePrefab { get; set; }
	
	public const float TimeOut = 3f;

	protected override void HandleComplexFiringInput()
	{
		if ( !Equipment.IsValid() || !Equipment.Grub.IsValid() )
			return;

		var grub = Equipment.Grub;
		if ( Input.Pressed( "fire" ) && !IsFiring )
		{
			IsFiring = true;
			TimeSinceLastUsed = 0f;
			
			FireHookTip();

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
				Log.Warning( "Trying to unmount, but ActiveMountable is null" );
				return;
			}
			
			grub.ActiveMountable.Dismount();
			FireFinished();
		}
	}

	private void FireHookTip()
	{
		if ( !ProjectilePrefab.IsValid() )
			return;

		SpawnProjectile( this, ProjectilePrefab, 75 );
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
		if ( IsFiring && Equipment.Grub?.ActiveMountable is null && TimeSinceLastUsed > TimeOut )
			FireFinished();
	}

	protected override void FireFinished()
	{
		if ( WeaponInfoPanel is not null )
		{
			WeaponInfoPanel.Inputs = new Dictionary<string, string>
			{
				{ "fire", "Fire Hook" }
			};
		}

		base.FireFinished();
	}
}
