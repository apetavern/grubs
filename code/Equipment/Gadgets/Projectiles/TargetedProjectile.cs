
using Grubs.Equipment.Weapons;
using System.ComponentModel.Design;

namespace Grubs.Equipment.Gadgets.Projectiles;

[Title( "Grubs - Targeted Projectile" ), Category( "Equipment" )]
public class TargetedProjectile : Projectile
{
	public Vector3 ProjectileTarget { get; set; }
	[Property] public Projectile ProjectileMovement { get; set; }

	public virtual void ShareData()
	{
		if ( Source != null )
		{
			ProjectileMovement.Source = Source;
			ProjectileMovement.Charge = Charge;
		}
		else
		{
			Source = ProjectileMovement.Source;
			Charge = ProjectileMovement.Charge;
		}
		ProjectileTarget = Source.Components.Get<TargetingWeapon>().ProjectileTarget;
	}
}
