namespace Grubs;

[Prefab, Category( "Drops" )]
public partial class Drop : ModelEntity, IResolvable
{
	[Prefab, Net]
	public float Size { get; set; } = 16f;

	public static Drop HealthCrate => SpawnDropOfType( "health_crate" );
	public static Drop WeaponCrate => SpawnDropOfType( "weapon_crate" );
	public static Drop ToolCrate => SpawnDropOfType( "tool_crate" );

	public bool Resolved => Velocity.IsNearlyZero( 2.5f );

	public TimeSince TimeSinceSpawned { get; set; } = 0f;

	private static readonly Material _spawnMaterial = Material.Load( "materials/effects/teleport/teleport.vmat" );

	public override void Spawn()
	{
		SetupPhysicsFromModel( PhysicsMotionType.Keyframed );
		EnableTouch = true;
		EnableAllCollisions = true;

		Tags.Add( "trigger" );

		TimeSinceSpawned = 0f;

		SetMaterialOverride( _spawnMaterial );
	}

	public override void Simulate( IClient client )
	{
		foreach ( var component in Components.GetAll<DropComponent>() )
		{
			component.Simulate( client );
		}
	}

	[Event.Client.Frame]
	public void UpdateEffects()
	{
		if ( TimeSinceSpawned > 2f )
		{
			ClearMaterialOverride();
		}
		else
		{
			SceneObject.Attributes.Set( "opacity", TimeSinceSpawned / 2 );
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

	public static Drop SpawnDropOfType( string crateType )
	{
		var dropPrefab = GetAllDropPrefabs().FirstOrDefault( drop => drop.ResourceName == crateType );
		if ( dropPrefab is null )
			return null;

		if ( !PrefabLibrary.TrySpawn<Drop>( dropPrefab.ResourcePath, out var drop ) )
			return null;

		return drop;
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
