namespace Grubs.Player;

public partial class GrubsPlayer : Entity
{
	[Net]
	public IList<Worm> Worms { get; set; } = new List<Worm>();

	[Net]
	public Worm ActiveWorm { get; set; }

	public CameraMode Camera
	{
		get => Components.Get<CameraMode>();
		set => Components.Add( value );
	}

	public GrubsPlayer()
	{
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
		if ( camera.Target == null )
		{
			camera.Target = Worms.First();
		}

		// Temporarily simulate the first (and only) worm.
		foreach ( var worm in Worms )
		{
			worm.Simulate( cl );
		}
	}

	public void CreateWorms()
	{
		if ( !IsServer )
			return;

		int wormsToSpawn = 2;
		List<Vector3> spawnPoints = GetSpawnLocations( wormsToSpawn );

		for ( int i = 0; i < wormsToSpawn; i++ )
		{
			var worm = new Worm();
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
