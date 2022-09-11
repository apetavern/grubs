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
	[Property, Category( "Weapon" )]
	public float ProjectileForceMultiplier { get; set; } = 1;

	[Property, Category( "Weapon" )]
	public bool ProjectileShouldBounce { get; set; } = false;

	[Property, Category( "Weapon" )]
	public bool ProjectileShouldUseTrace { get; set; } = true;

	[Property, Category( "Weapon" )]
	public int ProjectileMaxBounces { get; set; } = 0;

	[Property, Category( "Weapon" ), ResourceType( "vmdl" )]
	public string ProjectileModel { get; set; } = "";

	[Property, Category( "Weapon" )]
	public float ProjectileSpeed { get; set; } = 1000;

	[Property, Category( "Weapon" )]
	public float ProjectileExplosionRadius { get; set; } = 100;

	[Property, Category( "Weapon" )]
	public float ProjectileExplodeAfter { get; set; } = 0;

	[Property, Category( "Weapon" )]
	public float ProjectileCollisionExplosionDelay { get; set; } = 0;

	[Property, Category( "Weapon" ), ResourceType( "sound" )]
	public string ProjectileLoopSound { get; set; } = "";

	[Property, Category( "Weapon" ), ResourceType( "sound" )]
	public string ProjectileExplodeSound { get; set; } = "";

	[Property, Category( "Weapon" ), ResourceType( "vpcf" )]
	public string ProjectileParticleTrail { get; set; } = "";
}
