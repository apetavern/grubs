using Grubs.Equipment.Gadgets.Projectiles;

namespace Grubs.Equipment.Weapons;

public partial class Weapon
{
	[ActionGraphNode( "grubs.spawn_projectile" ), Title( "Spawn Projectile" ), Group( "Grubs Actions" )]
	public static void SpawnProjectile( Weapon source, GameObject projectile, int charge )
	{
		var go = projectile.Clone();
		go.NetworkSpawn();
		if ( go.Components.TryGet( out Projectile pc ) )
		{
			pc.Source = source;
			pc.Charge = charge;
		}

		source.TimeSinceLastUsed = 0f;
	}
}
