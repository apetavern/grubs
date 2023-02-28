namespace Grubs;

public partial class Player : Entity
{
	[Net]
	public IList<Grub> Grubs { get; private set; }

	[Net]
	public Grub ActiveGrub { get; private set; }

	[Net]
	public string SteamName { get; private set; }

	[Net]
	public long SteamId { get; private set; }

	public bool IsDead => Grubs.All( grub => grub.LifeState == LifeState.Dead );

	public bool IsAvailableForTurn => !IsDead && !IsDisconnected;

	[BindComponent]
	public GrubsCamera GrubsCamera { get; }

	[BindComponent]
	public Preferences Preferences { get; }

	[BindComponent]
	public Inventory Inventory { get; }

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
		SteamName = client.Name;
		SteamId = client.SteamId;
	}

	public override void Spawn()
	{
		Components.Create<GrubsCamera>();
		Components.Create<Preferences>();
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

	public void CreateGrubs()
	{
		for ( int i = 0; i < GrubsConfig.GrubCount; i++ )
		{
			Grubs.Add( new Grub( Client ) { Owner = this } );
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

	public int GetTotalGrubHealth()
	{
		return (int)Grubs.Sum( g => g.Health );
	}
}
