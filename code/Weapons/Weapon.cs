namespace Grubs;

[Prefab]
public partial class Weapon : AnimatedEntity
{
	/// <summary>
	/// The hold pose for this weapon.
	/// </summary>
	[Prefab, Net]
	public HoldPose HoldPose { get; set; } = HoldPose.None;

	/// <summary>
	/// The firing type for this weapon (instant or charged).
	/// </summary>
	[Prefab, Net]
	public FiringType FiringType { get; set; } = FiringType.Charged;

	/// <summary>
	/// The amount of times this weapon can be fired in succession 
	/// before the Grub's turn ends and an ammo is deducted.
	/// </summary>
	[Prefab, Net]
	public int Charges { get; set; } = 0;

	/// <summary>
	/// Whether the aim reticle should be shown for this weapon.
	/// </summary>
	[Prefab, Net]
	public bool ShowReticle { get; set; } = false;

	/// <summary>
	/// The default amount of ammo for a weapon. -1 means infinite.
	/// </summary>
	[Prefab, Net]
	public int DefaultAmmoAmount { get; set; } = 0;

	/// <summary>
	/// The amount of uses this weapon has.
	/// </summary>
	[Net]
	public int Ammo { get; set; }

	[Prefab, Net, ResourceType( "png" )]
	public string Icon { get; set; }

	public Grub Grub => Owner as Grub;

	public Weapon()
	{
		Transmit = TransmitType.Always;
	}

	public override void Spawn()
	{
		EnableDrawing = false;
	}

	public override void Simulate( IClient client )
	{
		SimulateComponents( client );

		DetermineWeaponVisibility();
	}

	public void OnDeploy( Grub grub )
	{
		EnableDrawing = true;

		SetParent( grub, true );
		Owner = grub;
	}

	public void OnHolster()
	{
		EnableDrawing = false;
	}

	protected void SimulateComponents( IClient client )
	{
		foreach ( var component in Components.GetAll<WeaponComponent>() )
		{
			component.Simulate( client );
		}
	}

	private void DetermineWeaponVisibility()
	{
		EnableDrawing = Grub.Controller.ShouldShowWeapon();
	}

	/// <summary>
	/// Spawns and returns a Weapon from the Prefab Library.
	/// </summary>
	/// <param name="prefabName">The asset path to the prefab.</param>
	/// <returns>The weapon if spawned successfully, otherwise null.</returns>
	public static Weapon FromPrefab( string prefabName )
	{
		if ( PrefabLibrary.TrySpawn<Weapon>( prefabName, out var weapon ) )
		{
			return weapon;
		}

		return null;
	}

	/// <summary>
	/// Returns all prefabs from the Prefab Library that are of Weapon type.
	/// </summary>
	/// <returns>A collection of Prefabs with the Weapon type.</returns>
	public static IEnumerable<Prefab> GetAllWeaponPrefabs()
	{
		return ResourceLibrary.GetAll<Prefab>()
			.Where( x => TypeLibrary.GetType( x.Root.Class ).TargetType == typeof( Weapon ) );
	}
}
