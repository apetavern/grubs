namespace Grubs;

public partial class Player : Entity
{
	[Net]
	public IList<Grub> Grubs { get; private set; }

	[Net]
	public Grub ActiveGrub { get; private set; }

	[Net]
	public IList<Gadget> Gadgets { get; private set; }

	[Net]
	public string SteamName { get; private set; }

	[Net]
	public long SteamId { get; private set; }

	public bool IsDead => Grubs.All( grub => grub.LifeState == LifeState.Dead );

	public bool IsAvailableForTurn => !IsDead && !IsDisconnected;

	[BindComponent]
	public Inventory Inventory { get; }

	public GrubsCamera GrubsCamera { get; } = new();

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

	public override void ClientSpawn()
	{
		if ( IsLocalPawn )
			SetDefaultGrubNames();
	}

	public override void Spawn()
	{
		Tags.Add( "ignorereset" );

		Components.Create<Inventory>();
	}

	public override void Simulate( IClient client )
	{
		Inventory.Simulate( client );
		Grubs.Simulate( client );
		Gadgets.Simulate( client );

		if ( IsTurn )
			ActiveGrub?.UpdateInputFromOwner( MoveInput, LookInput );
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
		var grubNames = string.IsNullOrEmpty( GrubNames ) ? new List<string>() : System.Text.Json.JsonSerializer.Deserialize<List<string>>( GrubNames );
		for ( int i = 0; i < GrubsConfig.GrubCount; i++ )
		{
			var grubName = grubNames.ElementAtOrDefault( i ) ?? Random.Shared.FromList( GrubNamePresets );
			Grubs.Add( new Grub( Client ) { Owner = this, Name = grubName } );
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
		if ( !ActiveGrub.IsValid() || !ActiveGrub.ActiveWeapon.IsValid() )
			return;

		if ( Inventory.ActiveWeapon.IsCharging() )
			Inventory.ActiveWeapon.Fire();

		Inventory.SetActiveWeapon( null, true );
	}

	public int GetTotalGrubHealth()
	{
		return (int)Grubs.Sum( g => g.Health );
	}
}
