namespace Grubs;

[Prefab, Category( "Drops" )]
public partial class Drop : ModelEntity
{
	[Prefab] public float Size { get; } = 12f;

	public Drop()
	{
		Transmit = TransmitType.Always;
	}

	public override void Spawn()
	{
		Log.Info( "Spawning Drop" );
	}

	public override void Simulate( IClient client )
	{
		foreach ( var component in Components.GetAll<DropComponent>() )
		{
			component.Simulate( client );
		}
	}
	
	

	public static IEnumerable<Prefab> GetAllDropPrefabs()
	{
		return ResourceLibrary.GetAll<Prefab>()
			.Where( x => TypeLibrary.GetType( x.Root.Class ).TargetType == typeof(Drop) );
	}

	[ConCmd.Admin( "gr_spawn_drop" )]
	public static void SpawnDrop()
	{
		foreach ( var prefab in GetAllDropPrefabs() )
		{
			if ( PrefabLibrary.TrySpawn<Drop>( prefab.ResourcePath, out var drop ) )
			{
				drop.Position = new Vector3( 0, 0, 0 );
				var player = Game.Clients.First().Pawn as Player;
				player?.Drops.Add( drop );
				drop.Owner = player;
			}
		}
	}
}
