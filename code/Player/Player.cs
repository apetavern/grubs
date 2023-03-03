namespace Grubs;

public partial class Player : Entity
{
	[Net]
	public IList<Grub> Grubs { get; private set; }

	[Net]
	public Grub ActiveGrub { get; private set; }

	[Net]
	public IList<Explosive> Explosives { get; private set; }

	[Net]
	public string SteamName { get; private set; }

	[Net]
	public long SteamId { get; private set; }

	public bool IsDead => Grubs.All( grub => grub.LifeState == LifeState.Dead );

	public bool IsAvailableForTurn => !IsDead && !IsDisconnected;

	[BindComponent]
	public Inventory Inventory { get; }

	public Preferences Preferences
	{
		get
		{
			return _preferences ??= Components.Get<Preferences>();
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

	public Player( IClient client, Preferences preferences ) : this()
	{
		CreateGrubs( client );

		SteamName = client.Name;
		SteamId = client.SteamId;

		Components.Add( preferences );
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
		SimulateExplosives( client );

		foreach ( var grub in Grubs )
		{
			grub.Simulate( client );
		}

		if ( IsTurn )
			ActiveGrub?.UpdateInputFromOwner( MoveInput, LookInput );
	}

	private void SimulateExplosives( IClient client )
	{
		for ( int i = Explosives.Count - 1; i >= 0; --i )
		{
			var explosive = Explosives[i];
			if ( explosive.IsValid() )
				explosive.Simulate( client );
			else
				Explosives.RemoveAt( i );
		}
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

	private void RotateGrubs()
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

		Inventory.ActiveWeapon.SetPointerEvents( To.Single( this ), false );

		Inventory.UnsetActiveWeapon();
	}

	public void AddExplosive( Explosive explosive )
	{
		Explosives.Add( explosive );
	}

	public int GetTotalGrubHealth()
	{
		return (int)Grubs.Sum( g => g.Health );
	}
}
