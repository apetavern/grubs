using Sandbox.Sdf;

namespace Grubs;

[Category( "Terrain" )]
public partial class Terrain : Entity
{
	/// <summary>
	/// The 2D SDF World that represents our terrain.
	/// </summary>
	[Net] public Sdf2DWorld SdfWorld { get; set; }

	/// <summary>
	/// The zone at the bottom of the map that instantly kills entities that fall into it.
	/// </summary>
	[Net] public DamageZone KillZone { get; set; }

	public override void Spawn()
	{
		Transmit = TransmitType.Always;

		InitializeSdfWorld();
	}

	/// <summary>
	/// Delete the existing SdfWorld and generate a new one.
	/// </summary>
	public void Reset()
	{
		SdfWorld?.Delete();
		InitializeSdfWorld();
	}

	protected void InitializeSdfWorld()
	{
		SetupKillZone();

		SdfWorld = new Sdf2DWorld()
		{
			LocalRotation = Rotation.FromRoll( 90f ),
		};
		SdfWorld.Tags.Add( "solid" );

		var creationStrategy = GrubsConfig.WorldTerrainType;
		switch ( creationStrategy )
		{
			case GrubsConfig.TerrainType.Generated:
				SetupGeneratedWorld();
				break;
			case GrubsConfig.TerrainType.Texture:
				SetupWorldFromTexture();
				break;
			default:
				SetupGeneratedWorld();
				break;
		}
	}

	protected void SetupKillZone()
	{
		KillZone?.Delete();

		// Need a custom value here since DebugOverlay.Box breaks with float.MaxValue.
		var maxValue = 2147483;

		var min = new Vector3( -maxValue, -64f / 2, -100 );
		var max = new Vector3( maxValue, 64f / 2, 0 );

		KillZone = new DamageZone()
			.WithDamageTags( "outofarea" )
			.WithSound( "water_splash" )
			.WithParticle( "particles/watersplash/watersplash_base.vpcf" )
			.WithDamage( 9999 )
			.WithPosition( Vector3.Zero )
			.WithBBox( new BBox( min, max ) )
			.Finish<DamageZone>();
	}

	public Vector3 FindSpawnLocation()
	{
		int retries = 0;
		while ( retries < 5000 )
		{
			var randX = Game.Random.Int( GrubsConfig.TerrainLength );
			var randZ = Game.Random.Int( GrubsConfig.TerrainHeight );
			var tr = Trace.Ray( new Vector3( randX, 0, randZ ), Vector3.Down * GrubsConfig.TerrainHeight )
				.WithAnyTags( "solid", "player" )
				.Radius( 16f )
				.Run();

			if ( tr.Hit && !tr.StartedSolid )
				return tr.EndPosition;
		}

		return new Vector3( 0f );
	}
}
