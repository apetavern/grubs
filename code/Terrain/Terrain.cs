﻿using Sandbox.Sdf;

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
		WorldTextureLength = 0;
		WorldTextureHeight = 0;

		SdfWorld?.ClearAsync();
		InitializeSdfWorld();
	}

	/// <summary>
	/// Clear the generated world and re-create with new environment.
	/// </summary>
	public void Refresh()
	{
		SdfWorld.ClearAsync();

		var cfg = new MaterialsConfig( true, true );
		var materials = GetActiveMaterials( cfg );
		AddWorldBox(
			GrubsConfig.TerrainLength,
			GrubsConfig.TerrainHeight,
			materials.ElementAt( 0 ).Key,
			materials.ElementAt( 1 ).Key );

		SubtractBackgroundBox( GrubsConfig.TerrainLength, (GrubsConfig.TerrainHeight / resolution).CeilToInt() );
		SubtractBackground( (GrubsConfig.TerrainLength / resolution).CeilToInt() );
		SubtractForeground( (GrubsConfig.TerrainLength / resolution).CeilToInt() );
	}

	protected void InitializeSdfWorld()
	{
		SetupKillZone();

		SdfWorld ??= new Sdf2DWorld();
		SdfWorld.LocalRotation = Rotation.FromRoll( 90f );
		SdfWorld.Tags.Add( Tag.Solid );

		ResetTerrainPosition();

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
			.WithDamageTags( Tag.OutOfArea )
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
	public Vector3 FindSpawnLocation( bool traceDown = true, ModelEntity? ent = null, float size = 16f )
	{
		int retries = 0;
		var existingGrubs = All.OfType<Grub>();
		var fallbackPosition = new Vector3();

		var maxWidth = GrubsConfig.TerrainLength;
		var maxHeight = GrubsConfig.TerrainHeight;
		if ( GrubsConfig.WorldTerrainType is GrubsConfig.TerrainType.Texture )
		{
			maxWidth = WorldTextureLength;
			maxHeight = WorldTextureHeight - 64;
		}

		while ( retries < 5000 )
		{
			var randX = Game.Random.Int( maxWidth ) - maxWidth / 2;
			var randZ = Game.Random.Int( maxHeight );
			var startPos = new Vector3( randX, 0, randZ );

			var tr = Trace.Ray( startPos, startPos + Vector3.Down * GrubsConfig.TerrainHeight )
				.WithAnyTags( Tag.Solid, Tag.Player )
				.Size( size )
				.Run();

			// If we're not tracing down, check the startpos, otherwise we'll clip.
			if ( ent is not null && !traceDown && IsInsideTerrain( startPos, ent ) )
				continue;

			if ( tr.Hit && !tr.StartedSolid )
			{
				if ( tr.Entity is Grub )
					continue;

				if ( IsInsideTerrain( tr.EndPosition, ent, size ) )
					continue;

				if ( Vector3.GetAngle( Vector3.Up, tr.Normal ) >= 70f )
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

		if ( retries >= 100 )
			throw new Exception( "Retry count is unusually high in Terrain::FindSpawnLocation." );

		return fallbackPosition;
	}

	public async Task LowerTerrain( float amount )
	{
		var grubs = All.OfType<Grub>();

		Vector3 targetPosition = SdfWorld.Position - Vector3.Up * amount;
		while ( Vector3.DistanceBetween( SdfWorld.Position, targetPosition ) > Time.Delta * 5f )
		{
			Camera.Main.Position += Vector3.Random * 10f;

			var oldPos = SdfWorld.Position;
			SdfWorld.Position = Vector3.Lerp( SdfWorld.Position, targetPosition, Time.Delta * 3f );

			// Lower Grubs with the terrain, otherwise they can clip into terrain above them.
			// I don't want to do this, but movement code is hard sometimes :(
			foreach ( var grub in grubs )
				grub.Position += Vector3.Up * (SdfWorld.Position.z - oldPos.z);

			await GameTask.DelaySeconds( Time.Delta );
		}
	}

	public void ResetTerrainPosition()
	{
		SdfWorld.Position = SdfWorld.Position.WithZ( 0 );
	}

	private bool IsInsideTerrain( Vector3 position, ModelEntity? ent = null, float size = 16f )
	{
		if ( ent is not null )
		{
			var boxTrace = Trace.Box( ent.CollisionBounds, position, position + Vector3.Right * 64f )
				.WithAnyTags( Tag.Solid )
				.Run();
			return boxTrace.Hit || boxTrace.StartedSolid;
		}

		var tr = Trace.Ray( position, position + Vector3.Right * 64f )
			.WithAnyTags( Tag.Solid )
			.Size( size )
			.Run();
		return tr.Hit;
	}

	private Vector3 GetRandomPositionInWorld( int length, int height )
	{
		var randX = Game.Random.Int( length ) - length / 2;
		var randZ = Game.Random.Int( height );
		return new Vector3( randX, 0, randZ );
	}

	private TimeSince TimeSinceLastWindEffect = 0f;

	[GameEvent.Tick.Client]
	private void ClientTick()
	{
		if ( TimeSinceLastWindEffect < 0.25f )
			return;

		// Do not show wind particles if there is no active wind.
		if ( GamemodeSystem.Instance.ActiveWindSteps == 0 )
			return;

		var pos = GetRandomPositionInWorld( GrubsConfig.TerrainLength, GrubsConfig.TerrainHeight ).WithY( -33 );
		var wind = Particles.Create( "particles/wind/wind_wisp_base.vpcf", pos );
		wind.SetPosition( 1, GamemodeSystem.Instance.ActiveWindSteps * 75f );
		TimeSinceLastWindEffect = 0f;
	}

	[ConCmd.Server]
	public static void RegenWorldServer()
	{
		GrubsGame.Instance.Terrain.Reset();
	}

	[ConCmd.Admin( "gr_regen" )]
	public static void RegenWorld()
	{
		GrubsGame.Instance.Terrain.Reset();
	}
}
