namespace Grubs;

[Prefab, Category( "Drops" )]
public partial class Drop : ModelEntity
{
	[Prefab, Net]
	public float Size { get; set; } = 16f;

	public override void Spawn()
	{
		SetupPhysicsFromModel( PhysicsMotionType.Keyframed );
		EnableTouch = true;
		EnableAllCollisions = true;

		Tags.Add( "trigger" );
	}

	public override void Simulate( IClient client )
	{
		foreach ( var component in Components.GetAll<DropComponent>() )
		{
			component.Simulate( client );
		}
	}

	public override void StartTouch( Entity other )
	{
		if ( Game.IsClient )
			return;

		foreach ( var component in Components.GetAll<DropComponent>() )
		{
			component.OnTouch( other );
		}
	}

	public static IEnumerable<Prefab> GetAllDropPrefabs()
	{
		return ResourceLibrary.GetAll<Prefab>()
			.Where( x => TypeLibrary.GetType( x.Root.Class ).TargetType == typeof( Drop ) );
	}

	[ConCmd.Admin( "gr_spawn_drop" )]
	public static void SpawnDrop()
	{
		var prefabs = GetAllDropPrefabs().ToList();
		var prefab = prefabs.ElementAt( Game.Random.Int( prefabs.Count - 1 ) );
		if ( !PrefabLibrary.TrySpawn<Drop>( prefab.ResourcePath, out var drop ) )
			return;

		drop.Position = new Vector3( 0, 0, 0 );
		var player = Game.Clients.First().Pawn as Player;
		player?.Drops.Add( drop );
		drop.Owner = player;
	}
}
