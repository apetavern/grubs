namespace Grubs.Weapons.Base;

/// <summary>
/// The base for all weapon definitions in Grubs.
/// </summary>
public class WeaponAsset : GameResource
{
	public static IReadOnlyList<WeaponAsset> All => _all;
	private static readonly List<WeaponAsset> _all = new();

	//
	// Meta
	//
	[Property, Category( "Meta" )]
	public string WeaponName { get; set; } = "Weapon";

	[Property, Category( "Meta" ), ResourceType( "vmdl" )]
	public string Model { get; set; } = "";

	[Property, Category( "Meta" ), ResourceType( "png" )]
	public string Icon { get; set; } = "";

	[Property, Category( "Meta" )]
	public FiringType FiringType { get; set; } = FiringType.Instant;

	[Property, Category( "Meta" )]
	public HoldPose HoldPose { get; set; } = HoldPose.None;

	[Property, Category( "Meta" )]
	public int Uses { get; set; } = 1;

	[Property, Category( "Meta" )]
	public float DropChance { get; set; } = 1;

	//
	// Aim related
	//
	[Property, Category( "Aim" )]
	public bool HasReticle { get; set; } = false;

	//
	// Weapon related
	//

	[Property, Category( "Weapon" )]
	public bool InfiniteAmmo { get; set; } = false;

	[Property, Category( "Weapon" ), ResourceType( "sound" )]
	public string FireSound { get; set; } = "";

	[Property, Category( "Weapon" ), ResourceType( "sound" )]
	public string DeploySound { get; set; } = "";

	protected override void PostLoad()
	{
		base.PostLoad();

		if ( !_all.Contains( this ) )
			_all.Add( this );
	}
}
