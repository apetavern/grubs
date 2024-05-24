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

	[ActionGraphNode( "grubs.spawn_homing_projectile" ), Title( "Spawn Homing Projectile" ), Group( "Grubs Actions" )]
	public static void SpawnHomingProjectile( HomingWeapon source, GameObject projectile, int charge )
	{
		var go = projectile.Clone();
		go.NetworkSpawn();
		if ( go.Components.TryGet( out HomingProjectile pc ) )
		{
			pc.Source = source;
			pc.Charge = charge;
			pc.PassDataOn();
			pc.ProjectileTarget = source.ProjectileTarget;
		}

		source.TimeSinceLastUsed = 0f;
	}

	[ActionGraphNode( "grubs.fire_finished" ), Title( "Fire Finished" ), Group( "Grubs Actions" )]
	public static void FireFinished( Weapon source )
	{
		source.FireFinished();
	}
}
