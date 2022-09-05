using Grubs.Utils;
using Grubs.Weapons.Base;

namespace Grubs.Player;

/// <summary>
/// A team of grubs.
/// </summary>
[Category( "Setup" )]
public partial class Team : Entity, ISpectator
{
	/// <summary>
	/// The list of clients that are a part of this team.
	/// </summary>
	[Net]
	public IList<Client> Clients { get; private set; }

	/// <summary>
	/// The list of grubs that are a part of this team.
	/// </summary>
	[Net]
	public IList<Grub> Grubs { get; private set; }

	/// <summary>
	/// The teams current client.
	/// <remarks>This will stay populated even after their turn has passed.</remarks>
	/// </summary>
	[Net]
	public Client ActiveClient { get; private set; }

	/// <summary>
	/// The teams current grub.
	/// <remarks>This will stay populated even after their turn has passed.</remarks>
	/// </summary>
	[Net]
	public Grub ActiveGrub { get; private set; }

	/// <summary>
	/// The teams weapon inventory.
	/// </summary>
	[Net]
	public GrubsInventory Inventory { get; private set; }

	/// <summary>
	/// The name of this team.
	/// </summary>
	[Net]
	public string TeamName { get; private set; }

	/// <summary>
	/// The index this team was given.
	/// </summary>
	[Net]
	public int TeamNumber { get; private set; }

	/// <summary>
	/// The index of the currently equipped weapon in the <see cref="Inventory"/>.
	/// </summary>
	[Net, Predicted]
	public int EquippedWeapon { get; private set; }

	/// <summary>
	/// The camera all team clients will see the game through.
	/// </summary>
	public CameraMode Camera
	{
		get => Components.Get<CameraMode>();
		private set => Components.Add( value );
	}

	/// <summary>
	/// Returns whether all grubs in this team are dead or not.
	/// </summary>
	public bool TeamDead => Grubs.All( grub => grub.LifeState == LifeState.Dead );

	/// <summary>
	/// Returns whether it is this teams turn.
	/// </summary>
	public bool IsTurn => TeamManager.Instance.CurrentTeam == this;

	public Team()
	{
		Transmit = TransmitType.Always;
	}

	public Team( List<Client> clients, string teamName, int teamNumber ) : this()
	{
		TeamName = teamName;
		TeamNumber = teamNumber;

		foreach ( var client in clients )
		{
			Clients.Add( client );
			client.Pawn = new Spectator();
		}

		Camera = new GrubsCamera();
		Inventory = new GrubsInventory
		{
			Owner = this
		};

		InitializeInventory();
		CreateGrubs();
	}

	public override void Simulate( Client cl )
	{
		base.Simulate( cl );

		if ( IsTurn && !GrubsGame.Current.CurrentGamemode.UsedTurn )
		{
			var lastWeapon = EquippedWeapon;

			if ( Input.Pressed( InputButton.Menu ) )
			{
				if ( EquippedWeapon == 0 )
					EquippedWeapon = Inventory.Items.Count - 1;
				else
					EquippedWeapon--;
			}

			if ( Input.Pressed( InputButton.Use ) )
			{
				if ( EquippedWeapon == Inventory.Items.Count - 1 )
					EquippedWeapon = 0;
				else
					EquippedWeapon++;
			}

			if ( EquippedWeapon != lastWeapon )
				ActiveGrub.EquipWeapon( Inventory.Items[EquippedWeapon] );
		}

		foreach ( var grub in Grubs )
			grub.Simulate( cl );
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		if ( !IsServer )
			return;

		foreach ( var grub in Grubs )
			grub.Delete();
	}

	/// <summary>
	/// Create and spawn the grubs for this team. Number of grubs spawned is defined by <see cref="GameConfig"/>.<see cref="GameConfig.GrubCount"/>.
	/// </summary>
	private void CreateGrubs()
	{
		Host.AssertServer();

		var grubsToSpawn = GameConfig.GrubCount;
		var spawnPoints = GetSpawnLocations( grubsToSpawn );

		for ( var i = 0; i < grubsToSpawn; i++ )
		{
			var grub = new Grub { Owner = this, Position = spawnPoints[i] };
			grub.Spawn( Clients.Count == 1 ? Clients[0] : null );

			Grubs.Add( grub );
		}

		ActiveGrub = Grubs.First();
	}

	private void InitializeInventory()
	{
		Host.AssertServer();

		foreach ( var assetDefinition in WeaponAsset.All )
		{
			var weapon = assetDefinition switch
			{
				MeleeWeaponAsset mWep => TypeLibrary.Create<GrubWeapon>( typeof( MeleeWeapon ), new object[] { mWep } ),
				ProjectileWeaponAsset pWep => TypeLibrary.Create<GrubWeapon>( typeof( ProjectileWeapon ), new object[] { pWep } ),
				_ => null
			};

			if ( weapon is null )
			{
				Log.Error( $"{assetDefinition.GetType()} is not a recognized {nameof( WeaponAsset )}" );
				continue;
			}

			Inventory.Add( weapon );
		}

		foreach ( var weapon in TypeLibrary.GetDescriptions<GrubWeapon>().Where( weapon => !weapon.IsAbstract ) )
			Inventory.Add( weapon.Create<GrubWeapon>() );
	}

	/// <summary>
	/// Sets the new client for this team.
	/// </summary>
	public void PickNextClient()
	{
		Host.AssertServer();

		if ( Clients[0].Pawn is not Spectator )
			Clients[0].Pawn = new Spectator();

		RotateClients();

		if ( Clients[0].Pawn is Spectator )
			Clients[0].Pawn.Delete();

		ActiveClient = Clients[0];
		ActiveClient.Pawn = this;
	}

	/// <summary>
	/// Rotate the clients list.
	/// </summary>
	private void RotateClients()
	{
		Host.AssertServer();

		var current = Clients[0];
		Clients.RemoveAt( 0 );
		Clients.Add( current );
	}

	/// <summary>
	/// Sets the new grub for this team.
	/// </summary>
	public void PickNextGrub()
	{
		Host.AssertServer();

		do
		{
			RotateGrubs();
			ActiveGrub = Grubs[0];
		} while ( ActiveGrub.LifeState == LifeState.Dead );
	}

	/// <summary>
	/// Rotate the grubs list.
	/// </summary>
	private void RotateGrubs()
	{
		Host.AssertServer();

		var current = Grubs[0];
		Grubs.RemoveAt( 0 );
		Grubs.Add( current );
	}

	private static List<Vector3> GetSpawnLocations( int num )
	{
		var spawnLocations = new List<Vector3>();
		while ( spawnLocations.Count < num )
			spawnLocations.Add( GrubsGame.Current.TerrainMap.GetSpawnLocation() );
		return spawnLocations;
	}
}
