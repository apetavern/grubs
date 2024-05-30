using Grubs.Equipment.Gadgets.Projectiles;
using Grubs.Equipment.Weapons;
using Sandbox;

namespace Grubs;

public sealed class DroppedProjectile : HomingProjectile
{

	public override void ShareData()
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
		ProjectileTarget = Source.Components.Get<HomingWeapon>().ProjectileTarget;
		Transform.Position = ProjectileTarget.WithZ( GrubsConfig.TerrainHeight * 2f );
	}
}
