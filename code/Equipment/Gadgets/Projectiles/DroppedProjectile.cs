using Grubs.Equipment.Gadgets.Projectiles;
using Grubs.Equipment.Weapons;
using Sandbox;

namespace Grubs;

public sealed class DroppedProjectile : TargetedProjectile
{

	public override void ShareData()
	{
		base.ShareData();
		Transform.Position = ProjectileTarget.WithZ( GrubsConfig.TerrainHeight * 2f );
	}
}
