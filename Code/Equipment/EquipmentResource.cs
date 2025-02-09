namespace Grubs.Equipment;

[GameResource( "Equipment", "geq", "Describes a piece of equipment", Icon = "hardware" )]
public class EquipmentResource : GameResource
{
	public string Name { get; set; } = "Equipment";
	public string Description { get; set; } = "Equipment definition.";
	[ResourceType( "jpg" )] public string Icon { get; set; } = "";
	public EquipmentType Type { get; set; } = EquipmentType.Weapon;

	public required PrefabFile EquipmentPrefab { get; set; }
	
	/// <summary>
	/// Whether the player's hat should be hidden while this equipment is deployed.
	/// </summary>
	public bool HideHatWhenDeployed { get; set; } = false;

	/// <summary>
	/// The default amount of ammo a piece of equipment has.
	/// -1 represents unlimited ammo.
	/// </summary>
	public int DefaultAmmo { get; set; } = 1;

	/// <summary>
	/// The chance of receiving this equipment in a crate.
	/// A chance of zero means it will not spawn from a crate.
	/// </summary>
	[Category( "Drops" )] public float DropChance { get; set; } = 1f;

	public static IEnumerable<EquipmentResource> All => ResourceLibrary.GetAll<EquipmentResource>();

	public static IEnumerable<EquipmentResource> AllOfType( EquipmentType type )
	{
		return ResourceLibrary.GetAll<EquipmentResource>().Where( res => res.Type == type );
	}
}

public enum EquipmentType
{
	Weapon,
	Tool
};
