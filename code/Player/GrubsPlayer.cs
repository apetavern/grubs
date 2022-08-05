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

	public CameraMode Camera
	{
		get => Components.Get<CameraMode>();
		set => Components.Add( value );
	}

	public GrubsPlayer( Client cl )
	{
		TeamNumber = GameConfig.TeamIndex++;

		CreateWorms( cl );
	}

	public GrubsPlayer()
	{
		
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

		var camera = Camera as GrubsCamera;
		camera.Target = Worms.First();

		// Simulate each worm.
		foreach ( var worm in Worms )
		{
			worm.Simulate( cl );
		}
	}

	public void CreateWorms(Client cl)
	{
		if ( !IsServer )
			return;

		int wormsToSpawn = GameConfig.WormCount;
		List<Vector3> spawnPoints = GetSpawnLocations( wormsToSpawn );

		for ( int i = 0; i < wormsToSpawn; i++ )
		{
			var worm = new Worm( TeamNumber );
			worm.Owner = this;
			worm.Spawn(cl);
			worm.Position = spawnPoints[i];

			Worms.Add( worm );
		}

		ActiveWorm = Worms.First();
		ActiveWorm.IsTurn = true;
	}

	public void InitializeInventory()
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
		var worldBounds = Map.Physics.Body.GetBounds();
		while ( spawnLocations.Count < num )
		{
			var location = worldBounds.RandomPointInside.WithZ( 1000 );
			var tr = Trace.Ray( location, location + Vector3.Down * 1000 ).WorldOnly().Run();
			if ( tr.Hit )
			{
				spawnLocations.Add( location.WithY( 0f ).WithZ( tr.EndPosition.z ) );
			}
		}
		return spawnLocations;
	}
}
