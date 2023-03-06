namespace Grubs;

[Prefab, Category( "Weapon" )]
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

	[Net]
	public int CurrentUses { get; set; } = 0;

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
	/// Whether the player is allowed to move while this weapon is firing.
	/// </summary>
	[Prefab, Net]
	public bool AllowMovement { get; set; } = false;

	/// <summary>
	/// Whether the player's aim should clamp to 45 degree angles while in use.
	/// </summary>
	[Prefab, Net]
	public bool ClampAim { get; set; } = false;

	/// <summary>
	/// The amount of uses this weapon has.
	/// </summary>
	[Net]
	public int Ammo { get; set; }

	[Net]
	public bool HasFired { get; set; } = false;

	[Net]
	public bool WeaponHasHat { get; set; }

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

		Ammo = DefaultAmmoAmount;
		WeaponHasHat = CheckWeaponForHat();
	}

	public override void Simulate( IClient client )
	{
		SimulateComponents( client );

		DetermineWeaponVisibility();
	}

	public void Deploy( Grub grub )
	{
		SetParent( grub, true );
		EnableDrawing = true;
		Owner = grub;

		foreach ( var component in Components.GetAll<WeaponComponent>() )
		{
			component.OnDeploy();
		}

		if ( FiringType is FiringType.Cursor )
			SetPointerEvents( true );
	}

	public void Holster( Grub grub )
	{
		SetParent( null );
		EnableDrawing = false;
		CurrentUses = 0;

		if ( HasFired && Ammo > 0 )
			Ammo--;

		HasFired = false;

		Log.Info( Game.IsClient );

		foreach ( var component in Components.GetAll<WeaponComponent>() )
		{
			component.OnHolster();
		}

		Grub.SetHatVisible( true );

		if ( FiringType is FiringType.Cursor )
			SetPointerEvents( false );
	}

	public void Fire()
	{
		foreach ( var component in Components.GetAll<WeaponComponent>() )
		{
			component.Fire();
		}
	}

	public bool IsFiring()
	{
		foreach ( var component in Components.GetAll<WeaponComponent>() )
		{
			if ( component.IsFiring )
				return true;
		}

		return false;
	}

	public bool IsCharging()
	{
		foreach ( var component in Components.GetAll<WeaponComponent>() )
		{
			if ( component.IsCharging )
				return true;
		}

		return false;
	}

	public bool HasAmmo()
	{
		return Ammo != 0;
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
		var show = Grub.Controller.ShouldShowWeapon();
		EnableDrawing = show;

		Grub.SetHatVisible( !WeaponHasHat || !show );
	}

	private bool CheckWeaponForHat()
	{
		for ( var i = 0; i < BoneCount; i++ )
		{
			if ( GetBoneName( i ) == "head" )
				return true;
		}

		return false;
	}

	private void SetPointerEvents( bool enabled )
	{
		if ( enabled )
			Event.Run( "pointer.enabled" );
		else
			Event.Run( "pointer.disabled" );
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

	[ClientRpc]
	public void PlayScreenSound( string sound )
	{
		this.SoundFromScreen( sound );
	}
}
