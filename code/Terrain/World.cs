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

	public List<Vector3> PossibleSpawnPoints = new();

	public readonly float WorldLength = GrubsConfig.TerrainLength;
	public readonly float WorldHeight = GrubsConfig.TerrainHeight;
	public const float WorldWidth = 64f;

	private readonly float _zoom = GrubsConfig.TerrainNoiseZoom;
	private readonly int _resolution = 16;
	private float[,] _terrainGrid;
	private const float GridSize = 1024f;

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

		CsgWorld.Add( CubeBrush, SandMaterial, scale: new Vector3( WorldLength, WorldWidth, WorldHeight ), position: new Vector3( 0, 0, -WorldHeight / 2 ) );
		CsgBackground.Add( CubeBrush, RockMaterial, scale: new Vector3( WorldLength, WorldWidth, WorldHeight ), position: new Vector3( 0, 64, -WorldHeight / 2 ) );

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

		SetupKillZone();
		SetupWater();
	}

	private void SetupKillZone()
	{
		var killBounds = new MultiShape().AddShape(
			BoxShape
			.WithSize( new Vector3( int.MaxValue, WorldWidth, 100 ) )
			.WithOffset( new Vector3( -int.MaxValue / 2, -WorldWidth / 2, -WorldHeight - 100 ) ) );

		KillZone = new DamageZone()
			.WithDamageTags( "outofarea" )
			.WithInstantKill( true )
			.WithDamage( 9999 )
			.WithPosition( Vector3.Zero )
			.WithShape( killBounds )
			.Finish<DamageZone>();
	}

	private void SetupWater()
	{
		var water = new Water();
		water.WaterMaterial = "materials/water/water_pond_a.vmat";

		var min = new Vector3( -WorldLength * 4, -WorldWidth * 16, -WorldHeight );
		var max = new Vector3( WorldLength * 4, WorldWidth * 16, -WorldHeight + 1 );
		water.CollisionBounds = new BBox( min, max );
		water.Position = new Vector3( 0, 0, 8 );
	}

	public Vector3 FindSpawnLocation()
	{
		int iterations = 0;
		while ( true && iterations < 10000 )
		{
			var startPos = Game.Random.FromList( PossibleSpawnPoints );
			var tr = Trace.Ray( startPos, startPos + Vector3.Down * WorldHeight ).WithTag( "solid" ).Run();
			if ( tr.Hit )
				return tr.EndPosition;
		}

		Log.Warning( "Couldn't find spawn location in 10,000 iterations." );
		return new Vector3( 0f );
	}

	[ConCmd.Admin( "gr_regen" )]
	public static void RegenWorld()
	{
		var world = GamemodeSystem.Instance.GameWorld;

		world.Reset();
	}
}
