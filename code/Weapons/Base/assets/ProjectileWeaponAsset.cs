namespace Grubs.Weapons.Base;

/// <summary>
/// Represents a weapon that is based upon <see cref="ProjectileWeapon"/>.
/// </summary>
[GameResource( "Grub Projectile Weapon Definition", "gpwep", "Describes a Grubs projectile weapon", Icon = "💀", IconBgColor = "#fe71dc", IconFgColor = "black" )]
public class ProjectileWeaponAsset : WeaponAsset
{
	//
	// Projectile weapon specific
	//
	[Property, Category( "Projectile" )]
	public float ProjectileForceMultiplier { get; set; } = 1;

	[Property, Category( "Projectile" )]
	public bool ProjectileShouldBounce { get; set; } = false;

	[Property, Category( "Projectile" )]
	public bool ProjectileShouldUseTrace { get; set; } = true;

	[Property, Category( "Projectile" )]
	public int ProjectileMaxBounces { get; set; } = 0;

	[Property, Category( "Projectile" ), ResourceType( "vmdl" )]
	public string ProjectileModel { get; set; }

	[Property, Category( "Projectile" )]
	public float ProjectileSpeed { get; set; } = 1000;

	[Property, Category( "Projectile" )]
	public float ProjectileExplosionRadius { get; set; } = 100;

	[Property, Category( "Projectile" )]
	public float ProjectileExplodeAfter { get; set; } = 0;

	[Property, Category( "Projectile" )]
	public float ProjectileCollisionExplosionDelay { get; set; } = 0;
}
