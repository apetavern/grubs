namespace Grubs.Equipment.Weapons;

[Title( "Ninja Rope Weapon" ), Category( "Equipment" )]
public class NinjaRopeWeapon : WeaponComponent
{
	protected override void HandleComplexFiringInput()
	{
		if ( Equipment.Grub is not { } grub )
			return;
		
		if ( Input.Pressed( "fire" ) && !IsFiring )
		{
			IsFiring = true;
			OnFire.Invoke( 100 );
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
}
