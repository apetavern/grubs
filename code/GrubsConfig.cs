namespace Grubs;

public static class GrubsConfig
{
	/// <summary>
	/// The Grubs gamemode to play.
	/// </summary>
	[ConVar( "grubs_game" )]
	public static string Gamemode { get; set; } = "FreeForAll";

	/// <summary>
	/// The minimum amount of players needed to start the game.
	/// </summary>
	[ConVar( "minimum_players" )]
	public static int MinimumPlayers { get; set; } = 2;

	/// <summary>
	/// If late joiners get a worm spawned in to play with.
	/// </summary>
	[ConVar( "gr_spawn_late_joiners" )]
	public static bool SpawnLateJoiners { get; set; } = false;

	// /// <summary>
	// /// The amount of <see cref="Grub"/>s that will be spawned per team.
	// /// </summary>
	[ConVar( "grub_count" )]
	public static int GrubCount { get; set; } = 1;

	// private static void OnGrubCountChange( int _, int _1 )
	// {
	// 	if ( Game.LocalPawn is Player player )
	// 		player.PopulateGrubNames();
	// }

	/// <summary>
	/// The max time in seconds that a player has to make their turn.
	/// </summary>
	[ConVar( "turn_duration" )]
	public static int TurnDuration { get; set; } = 45;

#if DEBUG
	/// <summary>
	/// If bot turns get used up instantly.
	/// </summary>
	[ConVar( "gr_bot_instantly_end_turn" )]
	public static bool InstantlyEndBotTurns { get; set; }
#endif

	/// <summary>
	/// Whether or not sudden death is enabled.
	/// </summary>
	[ConVar( "gr_sd_enabled" )]
	public static bool SuddenDeathEnabled { get; set; } = true;

	/// <summary>
	/// The amount of rounds to be played before Sudden Death begins.
	/// </summary>
	[ConVar( "gr_sd_delay" )]
	public static int SuddenDeathDelay { get; set; } = 6;

	/// <summary>
	/// Should all Grubs have their health set to 1 when Sudden Death begins?
	/// </summary>
	[ConVar( "gr_sd_onehealth" )]
	public static bool SuddenDeathOneHealth { get; set; } = false;

	public enum SuddenDeathAggressionAmount
	{
		Low = 15,
		Medium = 30,
		High = 50
	};

	/// <summary>
	/// How harshly Sudden Death affects the terrain.
	/// </summary>
	[ConVar( "gr_sd_aggression" )]
	public static SuddenDeathAggressionAmount SuddenDeathAggression { get; set; } = SuddenDeathAggressionAmount.Medium;

	/// <summary>
	/// Whether or not wind is enabled.
	/// </summary>
	[ConVar( "wind_enabled" )]
	public static bool WindEnabled { get; set; } = true;

	/// <summary>
	/// The step force of wind.
	/// </summary>
	[ConVar( "wind_force" )]
	public static float WindForce { get; set; } = 0.1f;

	/// <summary>
	/// Determines the range of how weak or strong the wind will be each turn.
	/// </summary>
	[ConVar( "wind_steps" )]
	public static int WindSteps { get; set; } = 3;

	/// <summary>
	/// The percent chance that a Weapon Crate will spawn every turn.
	/// </summary>
	[ConVar( "crate_weapon_chance" )]
	public static float WeaponCrateChancePerTurn { get; set; } = 0.05f;

	/// <summary>
	/// The percent chance that a Tools Crate will spawn every turn.
	/// </summary>
	[ConVar( "crate_tool_chance" )]
	public static float ToolCrateChancePerTurn { get; set; } = 0.05f;

	/// <summary>
	/// The percent chance that a Health Crate will spawn every turn.
	/// </summary>
	[ConVar( "crate_health_chance" )]
	public static float HealthCrateChancePerTurn { get; set; } = 0.05f;

	/// <summary>
	/// The number of landmines to attempt to spawn at the start
	/// </summary>
	[ConVar( "landmine_spawn_count" )]
	public static int LandmineSpawnCount { get; set; } = 5;

	/// <summary>
	/// The percent chance that a barrel will spawn.
	/// </summary>
	[ConVar( "barrel_chance" )]
	public static int BarrelChancePerTurn { get; set; } = 10;

	/// <summary>
	/// The time in seconds to give <see cref="Grub"/>s after using their turn.
	/// </summary>
	[ConVar( "movement_grace" )]
	public static float MovementGracePeriod { get; set; } = 5;

	/// <summary>
	/// The length of the terrain grid.
	/// </summary>
	[ConVar( "terrain_length" )]
	public static int TerrainLength { get; set; } = 2048;

	/// <summary>
	/// The height of the terrain grid.
	/// </summary>
	[ConVar( "terrain_height" )]
	public static int TerrainHeight { get; set; } = 1024;

	/// <summary>
	/// The zoom amount for the Perlin noise.
	/// </summary>
	[ConVar( "terrain_noise_zoom" )]
	public static float TerrainNoiseZoom { get; set; } = 2f;

	/// <summary>
	/// If true, we do not end the game when only one player is remaining.
	/// </summary>
	[ConVar( "keep_game_alive" )]
	public static bool KeepGameAlive { get; set; } = false;

	public enum TerrainEnvironmentType
	{
		Sand = 0,
		Dirt = 1,
		Cereal = 2
	}

	/// <summary>
	/// The environment type for the terrain (affects the materials used).
	/// </summary>
	[ConVar( "terrain_environment_type" )]
	public static TerrainEnvironmentType WorldTerrainEnvironmentType { get; set; } = TerrainEnvironmentType.Sand;

	public enum TerrainType
	{
		Generated = 0,
		Texture = 1
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
		Underground = 7
	}

	/// <summary>
	/// The type of terrain to use.
	/// </summary>
	[ConVar( "terrain_type" )]
	public static TerrainType WorldTerrainType { get; set; } = TerrainType.Generated;

	/// <summary>
	/// If texture is selected for TerrainType, TerrainTexture is used for determine which
	/// prefabricated texture is used to build the terrain.
	/// </summary>
	[ConVar( "terrain_texture" )]
	public static TerrainTexture WorldTerrainTexture { get; set; } = TerrainTexture.Cavern;

	/// <summary>
	/// The strength of the curves in the terrain's heightmap.
	/// </summary>
	[ConVar( "terrain_amplitude" )]
	public static float TerrainAmplitude { get; set; } = 48f;

	/// <summary>
	/// The frequency of the curves in the terrain's heightmap.
	/// </summary>
	[ConVar( "terrain_frequency" )]
	public static float TerrainFrequency { get; set; } = 0.5f;

	public static readonly List<string> PresetGrubNames = new()
	{
		"Froggy",
		"Balls",
		"Boggy",
		"Cammy",
		"Gibby",
		"Jaspy",
		"Ziks",
		"Wilson",
		"Winky",
		"Panini",
		"Perky",
		"Johnson",
		"Brie",
		"Scotty",
		"Nibbles",
		"Squirmy",
		"Tiny",
		"Wriggles",
		"Slippy",
		"Bumpy",
		"Chompy",
		"Slinky",
		"Grubsy",
		"Wormbert",
		"Wormbert Mk2",
		"Wormbert Jr",
		"Noodle",
		"Squiggly",
		"Twisty",
		"Blinky",
		"Gooey",
		"Snappy",
		"Slinky",
		"Gurgle",
		"Boopy",
		"Whimsy",
		"Squish",
		"Twitch",
		"Bubble",
		"Fuzzy",
		"Doodle",
		"Munchkin",
		"Pipsqueak",
		"Curly",
		"Pudgy",
		"Lumpy",
		"Pip",
		"Wobble",
		"Gizmo",
		"Dizzy",
		"Peanut",
		"Jelly"
	};

	/// <summary>
	/// The preset list of colors that players can pick from (TODO)
	/// For now, duplicate the list and random select from it
	/// </summary>
	public static readonly Dictionary<string, Color> PresetTeamColors = new()
	{
		{ "Green", Color.FromBytes(56, 229, 77) },
		{ "Pastel Green", Color.FromBytes(192, 255, 169) },
		{ "Orange", Color.FromBytes(255, 174, 109) },
		{ "Bright Yellow", Color.FromBytes(255, 216, 89) },
		{ "Yellow", Color.FromBytes(248, 249, 136) },
		{ "Cyan", Color.FromBytes(103, 234, 202) },
		{ "Pastel Brown", Color.FromBytes(118, 103, 87) },
		{ "Eggshell", Color.FromBytes(240, 236, 211) },
		{ "Red", Color.FromBytes(232, 59, 105) },
		{ "Strong Pink", Color.FromBytes(255, 129, 172) },
		{ "Pink", Color.FromBytes(251, 172, 204) },
		{ "Strong Purple", Color.FromBytes(213, 69, 255) },
		{ "Purple", Color.FromBytes(173, 162, 255) },
		{ "Blue", Color.FromBytes(33, 146, 255) },
		{ "Pastel Blue", Color.FromBytes(169, 213, 255) }
	};
}
