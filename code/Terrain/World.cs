using Sandbox.Csg;

namespace Grubs;

public partial class World : Entity
{
	[Net]
	public CsgSolid CsgWorld { get; set; }

	public CsgBrush CubeBrush { get; } = ResourceLibrary.Get<CsgBrush>( "brushes/cube.csg" );
	public CsgBrush CoolBrush { get; } = ResourceLibrary.Get<CsgBrush>( "brushes/cool.csg" );
	public CsgMaterial DefaultMaterial { get; } = ResourceLibrary.Get<CsgMaterial>( "materials/csg/default.csgmat" );
	public CsgMaterial SandMaterial { get; } = ResourceLibrary.Get<CsgMaterial>( "materials/csg/sand.csgmat" );

	private const float WorldLength = 2048f;
	private const float WorldWidth = 64f;
	private const float WorldHeight = 1024f;

	private const float GridSize = 1024f;

	public override void Spawn()
	{
		Assert.True( Game.IsServer );

		CsgWorld = new CsgSolid( GridSize );

		CsgWorld.Add( CubeBrush, SandMaterial, scale: new Vector3( WorldLength, WorldWidth, WorldHeight ), position: new Vector3( 0, 0, -WorldHeight / 2 ) );

		SubtractCube( -100, 100 );

		GenerateRandomWorld();
	}

	public void Reset()
	{
		CsgWorld?.Delete();

		CsgWorld = new CsgSolid( GridSize );

		CsgWorld.Add( CubeBrush, SandMaterial, scale: new Vector3( WorldLength, WorldWidth, WorldHeight ), position: new Vector3( 0, 0, -WorldHeight / 2 ) );

		GenerateRandomWorld();
	}

	public void SubtractCube( Vector3 min, Vector3 max )
	{
		CsgWorld.Subtract( CoolBrush, (min + max) * 0.5f, max - min );
	}

	float[,] TerrainGrid;
	int resolution = 32;
	int zoom = 16;

	public void GenerateRandomWorld()
	{
		var pointsX = (WorldLength / resolution).CeilToInt();
		var pointsZ = (WorldHeight / resolution).CeilToInt();
		var points = pointsX * pointsZ;

		Log.Info( $"pointsX: {pointsX}, pointsZ: {pointsZ}" );
		Log.Info( $"Total Points of Noise: {points}" );

		TerrainGrid = new float[pointsX, pointsZ];

		var r = Game.Random.Int( 99999 );

		// Initialize Simplex array of noise.
		for ( var x = 0; x < pointsX; x++ )
		{
			for ( var z = 0; z < pointsZ; z++ )
			{
				var n = Noise.Perlin( (x + r) * 3, r, (z + r) * 3 );
				n = Math.Abs( (n * 2) - 1 );
				TerrainGrid[x, z] = n;

				if ( TerrainGrid[x, z] < 0.1f )
				{
					var paddedRes = resolution + 24;
					var min = new Vector3( (x * resolution) - paddedRes, -64, (z * resolution) - paddedRes );
					var max = new Vector3( (x * resolution) + paddedRes, 64, (z * resolution) + paddedRes );
					min -= new Vector3( WorldLength / 2, 0, WorldHeight );
					max -= new Vector3( WorldLength / 2, 0, WorldHeight );
					SubtractCube( min, max );
				}
			}
		}
	}

	[ConCmd.Admin( "gr_regen" )]
	public static void RegenWorld()
	{
		var game = GrubsGame.Instance;
		var world = game.World;

		world.Reset();
	}
}
