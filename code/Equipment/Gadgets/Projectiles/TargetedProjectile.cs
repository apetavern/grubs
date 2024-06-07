using Grubs.Equipment.Weapons;

namespace Grubs.Equipment.Gadgets.Projectiles;

[Title( "Grubs - Targeted Projectile" ), Category( "Equipment" )]
public class TargetedProjectile : Projectile
{
	public Vector3 ProjectileTarget { get; set; }
	public Vector3 Direction { get; set; }
	[Property] public Projectile ProjectileMovement { get; set; }

	public virtual void ShareData()
	{
		if ( Source != null )
		{
			ProjectileMovement.SourceId = SourceId;
			ProjectileMovement.Charge = Charge;
		}
		else
		{
			SourceId = ProjectileMovement.SourceId;
			Charge = ProjectileMovement.Charge;
		}

		ProjectileTarget = Source.Components.Get<TargetingWeapon>().ProjectileTarget;
		Direction = Source.Components.Get<TargetingWeapon>().Direction;
	}
}
