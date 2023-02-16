namespace Grubs;

public partial class Player : Entity
{
	[Net]
	public IList<Grub> Grubs { get; private set; }

	[Net]
	public Grub ActiveGrub { get; private set; }

	public bool Dead => Grubs.All( grub => grub.LifeState == LifeState.Dead );

	[BindComponent]
	public Inventory Inventory { get; }

	public PlayerCamera Camera { get; private set; }

	// TODO: Allow the player to choose their own color.
	[Net]
	public Color Color { get; private set; } = Color.Random;

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

	public void PickNextGrub()
	{
		RotateGrubs();

		if ( Grubs[0].LifeState is LifeState.Dead or LifeState.Dying )
		{
			RotateGrubs();
		}

		ActiveGrub = Grubs[0];
	}

	public void RotateGrubs()
	{
		var current = Grubs[0];
		current.EyeRotation = Rotation.Identity;

		Grubs.RemoveAt( 0 );
		Grubs.Add( current );
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
