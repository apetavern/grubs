namespace Grubs;

[Prefab, Category( "Weapon" )]
public partial class Weapon : AnimatedEntity, IResolvable
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
	/// The chance of receiving this weapon in a crate.
	/// A chance of zero means it will not spawn from a crate.
	/// </summary>
	[Prefab, Net]
	public float DropChance { get; set; } = 1f;

	[Prefab, Net]
	public WeaponType WeaponType { get; set; } = WeaponType.Weapon;

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

	public bool Resolved => !IsFiring() && !IsCharging();

	public bool HasChargesRemaining => CurrentUses < Charges;

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

	[Net]
	public ModelEntity TargetIndicator { get; set; }

	public void Deploy( Grub grub )
	{
		SetParent( grub, true );
		EnableDrawing = true;
		Owner = grub;

		foreach ( var component in Components.GetAll<WeaponComponent>() )
		{
			component.OnDeploy();
		}

		UI.Cursor.Enabled( "Weapon", FiringType == FiringType.Cursor );
	}

	public void Holster( Grub _ )
	{
		EnableDrawing = false;
		CurrentUses = 0;

		if ( HasFired && Ammo > 0 )
			Ammo--;

		HasFired = false;

		foreach ( var component in Components.GetAll<WeaponComponent>() )
		{
			component.OnHolster();
		}

		Grub?.SetHatVisible( true );

		UI.Cursor.Enabled( "Weapon", FiringType == FiringType.Cursor );

		SetParent( null );
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

	public Vector3 GetStartPosition( bool isDroppable = false )
	{
		if ( isDroppable )
			return Position.WithY( 0 );

		var muzzle = GetAttachment( "muzzle" );
		if ( muzzle is null )
			return Grub.EyePosition;

		var tr = Trace.Ray( Grub.Controller.Hull.Center + Grub.Position, muzzle.Value.Position )
			.Ignore( this )
			.Ignore( Grub )
			.WithoutTags( "gadget" )
			.Radius( 1 )
			.Run();

		return tr.EndPosition;
	}

	public Vector3 GetMuzzlePosition()
	{
		var muzzle = GetAttachment( "muzzle" );
		if ( muzzle is null )
			return Grub.EyePosition;
		return muzzle.Value.Position;
	}

	public Vector3 GetMuzzleForward()
	{
		var muzzle = GetAttachment( "muzzle" );
		if ( muzzle is null )
			return Grub.EyeRotation.Forward * Grub.Facing;
		return muzzle.Value.Rotation.Forward;
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
		foreach ( var prefab in ResourceLibrary.GetAll<Prefab>() )
		{
			var prefabType = TypeLibrary.GetType( prefab.Root.Class );
			if ( prefabType is not null && prefabType.TargetType == typeof( Weapon ) )
			{
				yield return prefab;
			}
		}
	}

	[ClientRpc]
	public void PlayScreenSound( string sound )
	{
		this.SoundFromScreen( sound );
	}
}

public enum WeaponType
{
	Weapon,
	Tool
}
