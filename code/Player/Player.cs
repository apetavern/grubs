namespace Grubs;

public partial class Player : Entity
{
	[Net]
	public IList<Grub> Grubs { get; private set; }

	[Net]
	public Grub ActiveGrub { get; private set; }

	public bool IsDead => Grubs.All( grub => grub.LifeState == LifeState.Dead );

	[BindComponent]
	public Inventory Inventory { get; }

	public Preferences Preferences
	{
		get
		{
			return _preferences ??= Client.Components.Get<Preferences>();
		}
	}
	private Preferences _preferences;

	public bool IsTurn
	{
		get
		{
			return GamemodeSystem.Instance.ActivePlayer == this;
		}
	}

	public bool IsDisconnected => !Client.IsValid();

	public Player()
	{
		Transmit = TransmitType.Always;
	}

	public Player( IClient client ) : this()
	{
		CreateGrubs( client );
	}

	public override void Spawn()
	{
		// CreateGrubs();

		Components.Create<Inventory>();

		var weaponPrefabs = Weapon.GetAllWeaponPrefabs();
		foreach ( var prefab in weaponPrefabs )
		{
			if ( PrefabLibrary.TrySpawn<Weapon>( prefab.ResourcePath, out var weapon ) )
			{
				Inventory?.Add( weapon );
			}
		}
	}

	public override void Simulate( IClient client )
	{
		Inventory?.Simulate( client );

		foreach ( var grub in Grubs )
		{
			grub.Simulate( client );
		}

		if ( IsTurn )
			ActiveGrub?.UpdateInputFromOwner( MoveInput, LookInput );
	}

	public override void FrameSimulate( IClient client )
	{
		foreach ( var grub in Grubs )
		{
			grub.FrameSimulate( client );
		}
	}

	private void CreateGrubs( IClient client )
	{
		for ( int i = 0; i < GrubsConfig.GrubCount; i++ )
		{
			var grub = new Grub( client );
			grub.Owner = this;
			Grubs.Add( grub );
		}

		ActiveGrub = Grubs.First();
	}

	public void PickNextGrub()
	{
		RotateGrubs();

		while ( ActiveGrub.LifeState is LifeState.Dead or LifeState.Dying )
		{
			RotateGrubs();
		}
	}

	public void RotateGrubs()
	{
		var current = Grubs[0];
		current.EyeRotation = Rotation.Identity;

		Grubs.RemoveAt( 0 );
		Grubs.Add( current );

		ActiveGrub = Grubs[0];
	}

	public void EndTurn()
	{
		if ( ActiveGrub == null )
			return;

		if ( ActiveGrub.ActiveWeapon is null )
			return;

		Inventory.UnsetActiveWeapon();
	}

	public int GetTotalGrubHealth()
	{
		return (int)Grubs.Sum( g => g.Health );
	}
}
