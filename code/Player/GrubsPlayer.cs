using Grubs.Utils;

namespace Grubs.Player;

public partial class GrubsPlayer : Entity
{
	[Net]
	public IList<Worm> Worms { get; set; } = new List<Worm>();

	[Net]
	public Worm ActiveWorm { get; set; }

	[Net]
	public int TeamNumber { get; set; }

	public CameraMode Camera
	{
		get => Components.Get<CameraMode>();
		set => Components.Add( value );
	}

	public GrubsPlayer()
	{
		TeamNumber = GameConfig.TeamIndex++;

		CreateWorms();
	}

	public override void Spawn()
	{
		base.Spawn();

		Camera = new GrubsCamera();
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

	public void CreateWorms()
	{
		if ( !IsServer )
			return;

		int wormsToSpawn = GameConfig.WormCount;
		List<Vector3> spawnPoints = GetSpawnLocations( wormsToSpawn );

		for ( int i = 0; i < wormsToSpawn; i++ )
		{
			var worm = new Worm( TeamNumber );
			worm.Owner = this;
			worm.Spawn();
			worm.Position = spawnPoints[i];

			Worms.Add( worm );
		}

		Worms.First().IsTurn = true;
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
