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
		Tags.Add( "ignorereset" );

		Components.Create<GrubsCamera>();
		Components.Create<Preferences>();
		Components.Create<Inventory>();
	}

	public override void Simulate( IClient client )
	{
		Inventory?.Simulate( client );
		SimulateExplosives( client );

		foreach ( var grub in Grubs )
		{
			grub?.Simulate( client );
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
		GrubsCamera.FrameSimulate( client );

		foreach ( var grub in Grubs )
		{
			grub?.FrameSimulate( client );
		}
	}

	public void Respawn()
	{
		Inventory.Clear();
		Inventory.GiveDefaultLoadout();

		Grubs.Clear();
		CreateGrubs();
	}

	private void CreateGrubs()
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

		ActiveGrub.ActiveWeapon.Holster();
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
