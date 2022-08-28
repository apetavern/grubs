using Grubs.Utils;
using Grubs.Weapons;

namespace Grubs.Player;

[Category( "Setup" )]
public partial class Team : Entity, ISpectator
{
	/// <summary>
	/// The list of clients that are a part of this team.
	/// </summary>
	[Net]
	public IList<Client> Clients { get; private set; }

	/// <summary>
	/// The list of worms that are a part of this team.
	/// </summary>
	[Net]
	public IList<Worm> Worms { get; private set; }

	/// <summary>
	/// The teams current worm.
	/// <remarks>This will stay populated even after their turn has passed.</remarks>
	/// </summary>
	[Net, Predicted]
	public Worm ActiveWorm { get; private set; }

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
	/// The camera all team clients will see the game through.
	/// </summary>
	public CameraMode Camera
	{
		get => Components.Get<CameraMode>();
		private set => Components.Add( value );
	}

	/// <summary>
	/// Returns whether all worms in this team are dead or not.
	/// </summary>
	public bool TeamDead => Worms.All( worm => worm.LifeState == LifeState.Dead );

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
		CreateWorms();
	}

	public override void Simulate( Client cl )
	{
		base.Simulate( cl );

		foreach ( var worm in Worms )
			worm.Simulate( cl );
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		if ( !IsServer )
			return;

		foreach ( var worm in Worms )
			worm.Delete();
	}

	/// <summary>
	/// Create and spawn the worms for this team. Number of worms spawned is defined by <see cref="GameConfig"/>.<see cref="GameConfig.WormCount"/>.
	/// </summary>
	private void CreateWorms()
	{
		Host.AssertServer();

		var wormsToSpawn = GameConfig.WormCount;
		var spawnPoints = GetSpawnLocations( wormsToSpawn );

		for ( var i = 0; i < wormsToSpawn; i++ )
		{
			var worm = new Worm { Owner = this, Position = spawnPoints[i] };
			worm.Spawn( Clients.Count == 1 ? Clients[0] : null );

			Worms.Add( worm );
		}

		ActiveWorm = Worms.First();
	}

	private void InitializeInventory()
	{
		Host.AssertServer();

		foreach ( var weapon in TypeLibrary.GetTypes<GrubsWeapon>().Where( weapon => !weapon.IsAbstract ) )
			Inventory.Add( TypeLibrary.Create<GrubsWeapon>( weapon ) );
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
		
		Clients[0].Pawn = this;
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
	/// Sets the new worm for this team.
	/// </summary>
	public void PickNextWorm()
	{
		Host.AssertServer();

		RotateWorms();
		ActiveWorm = Worms[0];
	}

	/// <summary>
	/// Rotate the worms list.
	/// </summary>
	private void RotateWorms()
	{
		Host.AssertServer();

		var current = Worms[0];
		Worms.RemoveAt( 0 );
		Worms.Add( current );
	}

	private static List<Vector3> GetSpawnLocations( int num )
	{
		var spawnLocations = new List<Vector3>();
		while ( spawnLocations.Count < num )
			spawnLocations.Add( GrubsGame.Current.TerrainMap.GetSpawnLocation() );
		return spawnLocations;
	}
}
