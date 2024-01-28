namespace Grubs.Equipment.Weapons;

public partial class WeaponComponent
{
	[ActionGraphNode( "grubs.spawn_projectile" )]
	[Title( "Spawn Projectile" )]
	[Group( "Grubs Actions" )]
	public static void SpawnProjectile( WeaponComponent source, GameObject projectile, int charge )
	{
		var go = projectile.Clone();
		if ( go.Components.TryGet( out ProjectileComponent pc ) )
		{
			pc.Source = source;
			pc.Charge = charge;
		}
	}
}
