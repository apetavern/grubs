namespace Grubs.Weapons.Base;

/// <summary>
/// Represents a weapon that is based upon <see cref="HitscanWeapon"/>.
/// </summary>
[GameResource( "Grub Hitscan Weapon Definition", "ghwep", "Describes a Grubs hitscan weapon", Icon = "💀", IconBgColor = "#fe71dc", IconFgColor = "black" )]
public class HitscanWeaponAsset : WeaponAsset
{
	//
	// Hitscan weapon specific
	//
	[Property, Category( "Weapon" )]
	public float HitForce { get; set; } = 1f;

	[Property, Category( "Weapon" )]
	public int TraceCount { get; set; } = 1;

	[Property, Category( "Weapon" )]
	public float TraceSpread { get; set; } = 0f;

	[Property, Category( "Weapon" )]
	public float TraceDelay { get; set; } = 0f;

	[Property, Category( "Weapon" )]
	public float ExplosionRadius { get; set; } = 10f;

	[Property, Category( "Weapon" )]
	public float Damage { get; set; } = 25f;

	[Property, Category( "Weapon" )]
	public DamageFlags DamageFlags { get; set; }

	[Property, Category( "Weapon" )]
	public bool PenetrateTargets { get; set; } = false;

	[Property, Category( "Weapon" )]
	public bool PenetrateTerrain { get; set; } = false;
}
