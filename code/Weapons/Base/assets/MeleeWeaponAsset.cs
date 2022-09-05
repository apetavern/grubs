namespace Grubs.Weapons.Base;

/// <summary>
/// Represents a weapon that is based upon <see cref="MeleeWeapon"/>.
/// </summary>
[GameResource( "Grub Melee Weapon Definition", "gmwep", "Describes a Grubs melee weapon", Icon = "💀", IconBgColor = "#fe71dc", IconFgColor = "black" )]
public class MeleeWeaponAsset : WeaponAsset
{
	//
	// Melee weapon specific
	//
	[Property, Category( "Melee" )]
	public Vector3 HitSize { get; set; } = Vector3.One;

	[Property, Category( "Melee" )]
	public bool HitMulti { get; set; } = true;

	[Property, Category( "Melee" )]
	public float HitDelay { get; set; } = 1;

	[Property, Category( "Melee" )]
	public DamageFlags DamageFlags { get; set; } = DamageFlags.Blunt;

	[Property, Category( "Melee" )]
	public float HitForce { get; set; } = 100;

	[Property, Category( "Melee" )]
	public float Damage { get; set; } = 1;
}
