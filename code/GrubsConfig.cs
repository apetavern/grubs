namespace Grubs;

public static class GrubsConfig
{
	/// <summary>
	/// The Grubs gamemode to play.
	/// </summary>
	[ConVar.Replicated( "grubs_game" )]
	public static string Gamemode { get; set; } = "FreeForAll";

	/// <summary>
	/// The minimum amount of players needed to start the game.
	/// </summary>
	[ConVar.Replicated( "minimum_players" )]
	public static int MinimumPlayers { get; set; } = 2;

	/// <summary>
	/// If late joiners get a worm spawned in to play with.
	/// </summary>
	[ConVar.Replicated( "gr_spawn_late_joiners" )]
	public static bool SpawnLateJoiners { get; set; } = false;

	/// <summary>
	/// The amount of <see cref="Grub"/>s that will be spawned per team.
	/// </summary>
	[ConVar.Replicated( "grub_count" ), Change( nameof( OnGrubCountChange ) )]
	public static int GrubCount { get; set; } = 1;

	private static void OnGrubCountChange( int _, int _1 )
	{
		if ( Game.LocalPawn is Player player )
			player.PopulateGrubNames();
	}

	/// <summary>
	/// The max time in seconds that a player has to make their turn.
	/// </summary>
	[ConVar.Replicated( "turn_duration" )]
	public static int TurnDuration { get; set; } = 45;

	/// <summary>
	/// The amount of rounds to be played before Sudden Death begins.
	/// </summary>
	[ConVar.Replicated( "gr_sd_delay" )]
	public static int SuddenDeathDelay { get; set; } = 15;

	/// <summary>
	/// Should all Grubs have their health set to 1 when Sudden Death begins?
	/// </summary>
	[ConVar.Replicated( "gr_sd_onehealth" )]
	public static bool SuddenDeathOneHealth { get; set; } = false;

	/// <summary>
	/// How harshly Sudden Death affects the terrain.
	/// </summary>
	[ConVar.Replicated( "gr_sd_aggression" )]
	public static int SuddenDeathAggression { get; set; } = 30;

	/// <summary>
	/// Whether or not wind is enabled.
	/// </summary>
	[ConVar.Replicated( "wind_enabled" )]
	public static bool WindEnabled { get; set; } = true;

	/// <summary>
	/// The step force of wind.
	/// </summary>
	[ConVar.Replicated( "wind_force" )]
	public static float WindForce { get; set; } = 0.1f;

	/// <summary>
	/// Determines the range of how weak or strong the wind will be each turn.
	/// </summary>
	[ConVar.Replicated( "wind_steps" )]
	public static int WindSteps { get; set; } = 3;

	/// <summary>
	/// The percent chance that a Weapon Crate will spawn every turn.
	/// </summary>
	[ConVar.Replicated( "crate_weapon_chance" )]
	public static int WeaponCrateChancePerTurn { get; set; } = 5;

	/// <summary>
	/// The percent chance that a Tools Crate will spawn every turn.
	/// </summary>
	[ConVar.Replicated( "crate_tool_chance" )]
	public static int ToolCrateChancePerTurn { get; set; } = 5;

	/// <summary>
	/// The percent chance that a Health Crate will spawn every turn.
	/// </summary>
	[ConVar.Replicated( "crate_health_chance" )]
	public static int HealthCrateChancePerTurn { get; set; } = 5;

	/// <summary>
	/// The percent chance that a barrel will spawn.
	/// </summary>
	[ConVar.Replicated( "barrel_chance" )]
	public static int BarrelChancePerTurn { get; set; } = 10;

	/// <summary>
	/// The time in seconds to give <see cref="Grub"/>s after using their turn.
	/// </summary>
	[ConVar.Replicated( "movement_grace" )]
	public static float MovementGracePeriod { get; set; } = 5;

	/// <summary>
	/// The length of the terrain grid.
	/// </summary>
	[ConVar.Replicated( "terrain_length" )]
	public static int TerrainLength { get; set; } = 2048;

	/// <summary>
	/// The height of the terrain grid.
	/// </summary>
	[ConVar.Replicated( "terrain_height" )]
	public static int TerrainHeight { get; set; } = 1024;

	/// <summary>
	/// The zoom amount for the Perlin noise.
	/// </summary>
	[ConVar.Replicated( "terrain_noise_zoom" )]
	public static float TerrainNoiseZoom { get; set; } = 2f;

	public enum TerrainEnvironmentType
	{
		Sand = 0,
		Dirt = 1,
		Cereal = 2,
	}

	/// <summary>
	/// The environment type for the terrain (affects the materials used).
	/// </summary>
	[ConVar.Replicated( "terrain_environment_type" )]
	public static TerrainEnvironmentType WorldTerrainEnvironmentType { get; set; } = TerrainEnvironmentType.Sand;

	public enum TerrainType
	{
		Generated = 0,
		Texture = 1,
	}

	public enum TerrainTexture
	{
		Grubs = 0,
		Islands = 1,
		AntFarm = 2,
		Cavern = 3,
		Bunkers = 4,
		AICavern = 5,
		Hightower = 6,
	}

	/// <summary>
	/// The type of terrain to use.
	/// </summary>
	[ConVar.Replicated( "terrain_type" )]
	public static TerrainType WorldTerrainType { get; set; } = TerrainType.Generated;

	/// <summary>
	/// If texture is selected for TerrainType, TerrainTexture is used for determine which
	/// prefabricated texture is used to build the terrain.
	/// </summary>
	[ConVar.Replicated( "terrain_texture" )]
	public static TerrainTexture WorldTerrainTexture { get; set; } = TerrainTexture.Grubs;
}
