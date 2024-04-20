namespace Grubs.Equipment;

[GameResource( "Equipment", "geq", "Describes a piece of equipment", Icon = "hardware" )]
public class EquipmentResource : GameResource
{
	public string Name { get; set; } = "Equipment";
	public string Description { get; set; } = "Equipment definition.";
	public EquipmentType Type { get; set; } = EquipmentType.Weapon;

	public required PrefabFile EquipmentPrefab { get; set; }

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
