using Sandbox.Sdf;

namespace Grubs;

[Category( "Terrain" )]
public partial class Terrain : Entity
{
	/// <summary>
	/// The 2D SDF World that represents our terrain.
	/// </summary>
	public Sdf2DWorld SdfWorld { get; set; }

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
		SdfWorld.Clear();
		InitializeSdfWorld();
	}

	protected void InitializeSdfWorld()
	{
		SetupKillZone();

		SdfWorld ??= new Sdf2DWorld();
		SdfWorld.LocalRotation = Rotation.FromRoll( 90f );
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

	/// <summary>
	/// Find a spawn location for an entity. Attempts to sample for existing Grub locations.
	/// </summary>
	/// <returns>A Vector3 position where an entity can spawn.</returns>
	public Vector3 FindSpawnLocation( bool traceDown = true )
	{
		int retries = 0;
		var existingGrubs = All.OfType<Grub>();
		var fallbackPosition = new Vector3();

		var maxHeight = GrubsConfig.TerrainHeight;
		if ( GrubsConfig.WorldTerrainType is GrubsConfig.TerrainType.Texture )
			maxHeight = WorldTextureHeight - 64;

		while ( retries < 5000 )
		{
			var randX = Game.Random.Int( GrubsConfig.TerrainLength ) - GrubsConfig.TerrainLength / 2;
			var randZ = Game.Random.Int( maxHeight );
			var startPos = new Vector3( randX, 0, randZ );

			var tr = Trace.Ray( startPos, startPos + Vector3.Down * GrubsConfig.TerrainHeight )
				.WithAnyTags( "solid", "player" )
				.Size( 16f )
				.Run();

			if ( tr.Hit && !tr.StartedSolid )
			{
				if ( tr.Entity is Grub )
					continue;

				if ( IsInsideTerrain( tr.EndPosition ) )
					continue;

				if ( Vector3.GetAngle( Vector3.Up, tr.Normal ) > 70f )
					continue;

				fallbackPosition = tr.EndPosition;
				foreach ( var grub in existingGrubs )
				{
					if ( tr.EndPosition.Distance( grub.Position ) < 64f )
						continue;

					if ( !traceDown )
						return startPos;
					return tr.EndPosition;
				}
			}
			retries++;
		}

		return fallbackPosition;
	}

	private bool IsInsideTerrain( Vector3 position )
	{
		var tr = Trace.Ray( position, position + Vector3.Right * 64f )
			.WithAnyTags( "solid" )
			.Size( 1f )
			.Run();
		return tr.Hit;
	}

	[ConCmd.Admin( "gr_regen" )]
	public static void RegenWorld()
	{
		GrubsGame.Instance.Terrain.Reset();
	}
}
