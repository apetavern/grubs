using Grubs.Equipment.Gadgets.Projectiles;
using Grubs.Terrain;

namespace Grubs;

[Title( "Grubs - Dropped Projectile" ), Category( "Equipment" )]
public sealed class DroppedProjectile : TargetedProjectile
{
	public override void ShareData()
	{
		base.ShareData();
		Transform.Position = ProjectileTarget.WithZ( GrubsConfig.TerrainHeight * 2f );
	}	
}
