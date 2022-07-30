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
		Worms.First().Simulate( cl );
	}

	public void CreateWorms()
	{
		if ( !IsServer )
			return;

		for ( int i = 0; i < 1; i++ )
		{
			var worm = new Worm();
			worm.Owner = this;
			worm.Spawn();

			Worms.Add( worm );
		}
	}
}
