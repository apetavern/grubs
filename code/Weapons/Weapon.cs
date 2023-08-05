﻿namespace Grubs;

[Prefab, Category( "Weapon" )]
public partial class Weapon : AnimatedEntity, IResolvable
{
	#region Prefab

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
	/// Whether the player is allowed to move while this weapon is firing.
	/// </summary>
	[Prefab, Net]
	public bool AllowMovement { get; set; } = false;

	/// <summary>
	/// Whether this weapon should show input hints.
	/// </summary>
	[Prefab, Net]
	public bool ShowInputHints { get; set; } = true;

	/// <summary>
	/// The chance of receiving this weapon in a crate.
	/// A chance of zero means it will not spawn from a crate.
	/// </summary>
	[Prefab, Net]
	public float DropChance { get; set; } = 1f;

	/// <summary>
	/// The WeaponType of this weapon.
	/// </summary>
	[Prefab, Net]
	public WeaponType WeaponType { get; set; } = WeaponType.Weapon;

	[Prefab, Net]
	public bool CanSwapAfterUse { get; set; } = false;

	/// <summary>
	/// The amount of turns that must pass before this weapon can be used.
	/// </summary>
	[Prefab, Net]
	public int UnlockDelay { get; set; } = 0;

	public bool Unlocked => GamemodeSystem.Instance?.RoundsPassed >= UnlockDelay || Ammo == -1;

	/// <summary>
	/// If the weapon has a hat, override any Grub clothing.
	/// </summary>
	[Prefab, Net]
	public bool OverrideHat { get; set; } = true;

	/// <summary>
	/// The inventory icon for this weapon.
	/// </summary>
	[Prefab, Net, ResourceType( "png" )]
	public string Icon { get; set; }

	#endregion

	/// <summary>
	/// The amount of uses this weapon has.
	/// </summary>
	[Net]
	public int Ammo { get; set; }

	/// <summary>
	/// Whether or not the charge has been consumed for this weapon for the current turn.
	/// </summary>
	[Net]
	public bool IsChargeConsumed { get; set; } = false;

	/// <summary>
	/// The amount of times this weapon can be used for the current turn.
	/// </summary>
	[Net]
	public int CurrentUses { get; set; } = 0;

	/// <summary>
	/// Whether a weapon has a hat associated to dress the Grub in.
	/// </summary>
	[Net]
	public bool WeaponHasHat { get; set; }

	public Grub Grub => Owner as Grub;

	public bool Resolved => !IsFiring() && !IsCharging() || IsDormant;

	public bool HasChargesRemaining => CurrentUses < Charges;

	private UI.InputHintWorldPanel InputHintWorldPanel { get; set; }
	private bool _hasInputOverride;

	public Weapon()
	{
		Transmit = TransmitType.Always;
	}

	public override void Spawn()
	{
		EnableDrawing = false;

		Ammo = DefaultAmmoAmount;
		WeaponHasHat = OverrideHat && CheckWeaponForHat();
	}

	public override void Simulate( IClient client )
	{
		SimulateComponents( client );
		DetermineWeaponVisibility();

		if ( Game.IsClient && !_hasInputOverride )
			InputHintWorldPanel?.UpdateInput( InputAction.Fire, GetFireInputActionDescription() );
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

		if ( Game.IsServer )
			return;

		UI.Cursor.Enabled( "Weapon", FiringType == FiringType.Cursor );

		if ( !ShowInputHints )
			return;

		var inputHintOverride = Components.Get<InputHintOverrideComponent>();
		_hasInputOverride = inputHintOverride is not null;

		var inputActions = _hasInputOverride ? inputHintOverride.InputActions.ToList() : new List<string>() { InputAction.Fire };
		var inputDescriptions = _hasInputOverride ? inputHintOverride.InputDescriptions.ToList() : new List<string>() { GetFireInputActionDescription() };

		InputHintWorldPanel = new UI.InputHintWorldPanel( grub, inputActions, inputDescriptions );
	}

	public void Holster( Grub _ )
	{
		EnableDrawing = false;
		CurrentUses = 0;

		if ( IsChargeConsumed && Ammo > 0 )
			Ammo--;

		IsChargeConsumed = false;

		foreach ( var component in Components.GetAll<WeaponComponent>() )
		{
			component.OnHolster();
		}

		Grub?.SetHatVisible( true );

		UI.Cursor.Enabled( "Weapon", false );

		if ( Game.IsClient )
			InputHintWorldPanel?.Delete();

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

	public bool IsAvailable()
	{
		return HasAmmo() && Unlocked;
	}

	public Vector3 GetStartPosition( bool isDroppable = false )
	{
		if ( FiringType == FiringType.Cursor )
			return Grub.Player.MousePosition;

		if ( isDroppable )
			return Position.WithY( 0 );

		var muzzle = GetAttachment( "muzzle" );
		if ( muzzle is null )
			return Grub.EyePosition;

		var tr = Trace.Ray( Grub.Controller.Hull.Center + Grub.Position, muzzle.Value.Position )
			.Ignore( this )
			.Ignore( Grub )
			.WithoutTags( Tag.Gadget )
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

	private bool HasAmmo()
	{
		return Ammo != 0;
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

	private string GetFireInputActionDescription()
	{
		return FiringType switch
		{
			FiringType.Instant => "Fire",
			FiringType.Charged => "Fire (Hold)",
			FiringType.Cursor => "Set Target",
			_ => string.Empty,
		};
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
}

public enum WeaponType
{
	Weapon,
	Tool
}
