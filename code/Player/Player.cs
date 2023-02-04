namespace Grubs;

public partial class Player : Entity
{
	[Net]
	public IList<Grub> Grubs { get; private set; }

	[Net]
	public Grub ActiveGrub { get; private set; }

	[BindComponent]
	public Inventory Inventory { get; }

	public PlayerCamera Camera { get; private set; }

	public bool IsTurn
	{
		get
		{
			return GamemodeSystem.Instance.ActivePlayer == this;
		}
	}

	public Player()
	{
		Transmit = TransmitType.Always;
	}

	public override void Spawn()
	{
		CreateGrubs();

		Components.Create<Inventory>();
		// Inventory?.Add( Weapon.FromPrefab( "weapons/bazooka.prefab" ), true );

		if ( Game.IsClient )
			Camera = new PlayerCamera();
	}

	public override void Simulate( IClient client )
	{
		// Inventory?.Simulate( client );

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

		if ( Camera is null )
			Camera = new PlayerCamera();

		Camera?.SetTarget( GamemodeSystem.Instance.CameraTarget );
		Camera?.UpdateCamera( this );
	}

	private void CreateGrubs()
	{
		for ( int i = 0; i < GrubsConfig.GrubCount; i++ )
		{
			var grub = new Grub();
			grub.Owner = this;
			Grubs.Add( grub );
		}

		ActiveGrub = Grubs.First();
	}

	public void RotateGrubs()
	{
		var current = Grubs[0];
		current.EyeRotation = Rotation.Identity;

		Grubs.RemoveAt( 0 );
		Grubs.Add( current );

		ActiveGrub = Grubs[0];
	}
}
