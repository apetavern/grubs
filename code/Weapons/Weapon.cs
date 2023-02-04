namespace Grubs;

[Prefab]
public partial class Weapon : AnimatedEntity
{
	/// <summary>
	/// The amount of uses this weapon has.
	/// </summary>
	[Net]
	public int Ammo { get; set; }

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

	/// <summary>
	/// The hold pose for this weapon.
	/// </summary>
	[Prefab]
	public HoldPose HoldPose { get; set; } = HoldPose.None;

	/// <summary>
	/// The firing type for this weapon (instant or charged).
	/// </summary>
	[Prefab]
	public FiringType FiringType { get; set; } = FiringType.Charged;

	/// <summary>
	/// The amount of times this weapon can be fired in succession 
	/// before the Grub's turn ends and an ammo is deducted.
	/// </summary>
	[Prefab]
	public int Charges { get; set; } = 0;

	/// <summary>
	/// Whether the aim reticle should be shown for this weapon.
	/// </summary>
	[Net, Prefab]
	public bool ShowReticle { get; set; } = false;

	/*	/// <summary>
		/// Spawns and returns a Weapon from the Prefab Library.
		/// </summary>
		/// <param name="prefabName">The asset path to the prefab.</param>
		/// <returns>The weapon if spawned successfully, otherwise null.</returns>*/
	public static Weapon FromPrefab( string prefabName )
	{
		if ( PrefabLibrary.TrySpawn<Weapon>( prefabName, out var weapon ) )
		{
			return weapon;
		}

		return null;
	}
}
