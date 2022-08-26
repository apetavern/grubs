using Grubs.States;
using Grubs.Utils;
using Grubs.Weapons;

namespace Grubs.Player;

public partial class GrubsPlayer : Entity
{
	[Net]
	public IList<Worm> Worms { get; set; } = new List<Worm>();

	[Net]
	public Worm ActiveWorm { get; set; }

	[Net, Local]
	public GrubsInventory Inventory { get; set; }

	[Net]
	public int TeamNumber { get; set; }

	public bool IsTurn
	{
		get
		{
			if ( GrubsGame.Current.CurrentState is not PlayState playState )
				return false;

			return playState.TeamsTurn == TeamNumber;
		}
	}

	public CameraMode Camera
	{
		get => Components.Get<CameraMode>();
		set => Components.Add( value );
	}

	public bool TeamDead => Worms.Count == 0;

	public GrubsPlayer()
	{
		Transmit = TransmitType.Always;
	}

	public GrubsPlayer( Client cl ) : this()
	{
		TeamNumber = GameConfig.TeamIndex++;

		CreateWorms( cl );
	}

	public override void Spawn()
	{
		base.Spawn();

		Camera = new GrubsCamera();

		Inventory = new GrubsInventory()
		{
			Owner = this
		};

		InitializeInventory();
	}

	public override void Simulate( Client cl )
	{
		base.Simulate( cl );

		// Simulate each worm.
		foreach ( var worm in Worms )
		{
			worm.Simulate( cl );
		}
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
	/// Create and spawn the worms for this player. Number of worms spawned
	/// is defined by WormCount from GameConfig.
	/// </summary>
	/// <param name="cl">The client associated with this player.</param>
	public void CreateWorms( Client cl )
	{
		if ( !IsServer )
			return;

		int wormsToSpawn = GameConfig.WormCount;
		List<Vector3> spawnPoints = GetSpawnLocations( wormsToSpawn );

		for ( int i = 0; i < wormsToSpawn; i++ )
		{
			var worm = new Worm { Owner = this, Position = spawnPoints[i] };
			worm.Spawn( cl );

			Worms.Add( worm );
		}

		ActiveWorm = Worms.First();
	}

	private void InitializeInventory()
	{
		var weapons = TypeLibrary.GetTypes<GrubsWeapon>()
			.Where( weapon => !weapon.IsAbstract );

		foreach ( var weapon in weapons )
		{
			Inventory.Add( TypeLibrary.Create<GrubsWeapon>( weapon ) );
		}
	}

	private static List<Vector3> GetSpawnLocations( int num )
	{
		var spawnLocations = new List<Vector3>();
		// var worldBounds = Map.Physics.Body.GetBounds();
		while ( spawnLocations.Count < num )
		{
			spawnLocations.Add( GrubsGame.Current.TerrainMap.GetSpawnLocation() );
			/*var location = worldBounds.RandomPointInside.WithZ( 1000 );
			var tr = Trace.Ray( location, location + Vector3.Down * 1000 ).WorldOnly().Run();
			if ( tr.Hit )
			{
				spawnLocations.Add( location.WithY( 0f ).WithZ( tr.EndPosition.z ) );
			}*/
		}
		return spawnLocations;
	}

	/// <summary>
	/// Set a new ActiveWorm from the worms list.
	/// </summary>
	public void PickNextWorm()
	{
		RotateWorms();
		ActiveWorm = Worms[0];
	}

	/// <summary>
	/// Rotate the worms list.
	/// </summary>
	private void RotateWorms()
	{
		var current = Worms[0];
		Worms.RemoveAt( 0 );
		Worms.Add( current );
	}
}
