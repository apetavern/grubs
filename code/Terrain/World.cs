using Grubs.Terrain.CSG;
using Sandbox.Csg;

namespace Grubs;

[Category( "Terrain" )]
public partial class World : Entity
{
	[Net]
	public CsgSolid CsgWorld { get; set; }

	[Net]
	public CsgSolid CsgBackground { get; set; }

	[Net]
	public DamageZone KillZone { get; set; }

	/*
	 * Brushes
	 */
	public CsgBrush CubeBrush { get; } = ResourceLibrary.Get<CsgBrush>( "brushes/cube.csg" );
	public CsgBrush CoolBrush { get; } = ResourceLibrary.Get<CsgBrush>( "brushes/cool.csg" );

	public CsgBrushPrefab PrefabBrush { get; set; }

	/*
	 * Materials
	 */
	public CsgMaterial DefaultMaterial { get; } = ResourceLibrary.Get<CsgMaterial>( "materials/csg/default.csgmat" );
	public CsgMaterial AltSandMaterial { get; } = ResourceLibrary.Get<CsgMaterial>( "materials/csg/sand_a.csgmat" );
	public CsgMaterial SandMaterial { get; } = ResourceLibrary.Get<CsgMaterial>( "materials/csg/sand_b.csgmat" );
	public CsgMaterial LavaMaterial { get; } = ResourceLibrary.Get<CsgMaterial>( "materials/csg/lava.csgmat" );
	public CsgMaterial RockMaterial { get; } = ResourceLibrary.Get<CsgMaterial>( "materials/csg/rocks_a.csgmat" );
	public CsgMaterial MetalMaterial { get; } = ResourceLibrary.Get<CsgMaterial>( "materials/csg/metal_a.csgmat" );

	public List<Vector3> PossibleSpawnPoints = new();

	public readonly float WorldLength = GrubsConfig.TerrainLength;
	public readonly float WorldHeight = GrubsConfig.TerrainHeight;
	public const float WorldWidth = 64f;

	private readonly float _zoom = GrubsConfig.TerrainNoiseZoom;
	private readonly int _resolution = 16;
	private float[,] _terrainGrid;
	private const float GridSize = 1024f;

	public static bool IsResolved()
	{
		return All.OfType<IResolvable>().All( ent => ent.Resolved );
	}

	public static async Task UntilResolve()
	{
		while ( !IsResolved() )
			await GameTask.Delay( 300 );
	}

	public override void Spawn()
	{
		Reset();
	}

	public void Reset()
	{
		CsgWorld?.Delete();
		CsgBackground?.Delete();

		CsgWorld = new CsgSolid( GridSize );
		CsgBackground = new CsgSolid( GridSize );

		switch ( GrubsConfig.WorldTerrainType )
		{
			case GrubsConfig.TerrainType.Generated:
				SetupGenerateWorld();
				break;
			case GrubsConfig.TerrainType.Texture:
				SetupTextureWorld();
				break;
			default:
				SetupGenerateWorld();
				break;
		}
	}

	private void SetupKillZone( float height )
	{
		// Need a custom value here since DebugOverlay.Box breaks with float.MaxValue.
		var maxValue = 2147483;

		var min = new Vector3( -maxValue, -WorldWidth / 2, -height - 100 );
		var max = new Vector3( maxValue, WorldWidth / 2, -height );

		KillZone = new DamageZone()
			.WithDamageTags( "outofarea" )
			.WithDamage( 9999 )
			.WithPosition( Vector3.Zero )
			.WithBBox( new BBox( min, max ) )
			.Finish<DamageZone>();
	}

	private void SetupWater( float length, float height )
	{
		var water = new Water();
		water.WaterMaterial = "materials/water/water_pond_a.vmat";

		var min = new Vector3( -length * 4, -WorldWidth * 16, -height );
		var max = new Vector3( length * 4, WorldWidth * 16, -height + 1 );
		water.CollisionBounds = new BBox( min, max );
		water.Position = new Vector3( 0, 0, 8 );
	}

	public Vector3 FindSpawnLocation()
	{
		int iterations = 0;
		while ( true && iterations < 10000 )
		{
			var startPos = Game.Random.FromList( PossibleSpawnPoints );
			var tr = Trace.Ray( startPos, startPos + Vector3.Down * WorldHeight )
				.Size( 1f )
				.WithAnyTags( "solid", "gadget" )
				.Run();

			if ( tr.Hit )
			{
				return tr.EndPosition;
			}
		}

		Log.Warning( "Couldn't find spawn location in 10,000 iterations." );
		return new Vector3( 0f );
	}

	[ConCmd.Admin( "gr_spawn_check" )]
	public static void SpawnCheck()
	{
		var world = GamemodeSystem.Instance.GameWorld;
		var spawnLocation = world.FindSpawnLocation();
		DebugOverlay.Sphere( spawnLocation, 16f, Color.Random, 16f );
		Log.Info( "SpawnCheck: " + spawnLocation );
	}

	[ConCmd.Admin( "gr_trace_check" )]
	public static void TraceCheck( float x, float y, float z )
	{
		var vec = new Vector3( x, y, z );
		var tr = Trace.Ray( vec, vec + Vector3.Down * GamemodeSystem.Instance.GameWorld.WorldHeight )
				.WithAnyTags( "solid", "gadget" )
				.Run();
		DebugOverlay.TraceResult( tr, 10f );
		Log.Info( tr.EndPosition );
		Log.Info( tr.StartedSolid );
	}

	[ConCmd.Admin( "gr_regen" )]
	public static void RegenWorld()
	{
		var world = GamemodeSystem.Instance.GameWorld;

		world.Reset();
	}
}
