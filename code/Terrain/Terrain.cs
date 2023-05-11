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

	public Sdf2DMaterial DevMaterial { get; } = ResourceLibrary.Get<Sdf2DMaterial>( "materials/sdf2d_default.sdflayer" );

	public override void Spawn()
	{
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

		SdfWorld = new Sdf2DWorld( Sdf2DWorldQuality.Medium )
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
		var tr = Trace.Ray( new Vector3( 0, 0, GrubsConfig.TerrainHeight + 64f ), Vector3.Down * GrubsConfig.TerrainHeight )
			.WithAnyTags( "solid" )
			.Radius( 1f )
			.Run();
		return tr.Hit ? tr.EndPosition : new Vector3( 0f );
	}
}
