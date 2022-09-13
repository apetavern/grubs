using Grubs.Crates;
using Grubs.Player;

namespace Grubs.Utils;

public static class GameConfig
{
	/// <summary>
	/// The Grubs gamemode to play.
	/// </summary>
	[ConVar.Replicated( "grubs_game" )]
	public static string Gamemode { get; set; } = "ffa";

	/// <summary>
	/// The minimum amount of players needed to start the game.
	/// </summary>
	[ConVar.Replicated( "minimum_players" )]
	public static int MinimumPlayers { get; set; } = 2;

	/// <summary>
	/// The maximum amount of players that can be in the game.
	/// </summary>
	[ConVar.Replicated( "maximum_players" )]
	public static int MaximumPlayers { get; set; } = 4;

	/// <summary>
	/// The amount of <see cref="Grub"/>s that will be spawned per team.
	/// </summary>
	[ConVar.Replicated( "grub_count" )]
	public static int GrubCount { get; set; } = 4;

	/// <summary>
	/// The max time in seconds that a player has to make their turn.
	/// </summary>
	[ConVar.Replicated( "turn_duration" )]
	public static int TurnDuration { get; set; } = 60;

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
	/// The percent chance that a <see cref="WeaponCrate"/> will spawn every turn.
	/// </summary>
	[ConVar.Replicated( "crate_weapon_chance" )]
	public static int WeaponCrateChancePerTurn { get; set; } = 10;

	/// <summary>
	/// The percent chance that a <see cref="HealthCrate"/> will spawn every turn.
	/// </summary>
	[ConVar.Replicated( "crate_health_chance" )]
	public static int HealthCrateChancePerTurn { get; set; } = 5;

	/// <summary>
	/// The time in seconds to give <see cref="Grub"/>s after using their turn.
	/// </summary>
	[ConVar.Replicated( "movement_grace" )]
	public static float MovementGracePeriod { get; set; } = 5;

	/// <summary>
	/// The type of material to be used for the terrain.
	/// </summary>
	[ConVar.Replicated( "terrain_type" )]
	public static string TerrainType { get; set; } = "sand";

	/// <summary>
	/// The width of the terrain grid.
	/// </summary>
	[ConVar.Replicated( "terrain_width" )]
	public static int TerrainWidth { get; set; } = 150;

	/// <summary>
	/// The height of the terrain grid.
	/// </summary>
	[ConVar.Replicated( "terrain_height" )]
	public static int TerrainHeight { get; set; } = 100;

	/// <summary>
	/// The scale of the terrain.
	/// </summary>
	[ConVar.Replicated( "terrain_scale" )]
	public static int TerrainScale { get; set; } = 25;

	/// <summary>
	/// The resolution of the noise for the terrain.
	/// </summary>
	[ConVar.Replicated( "terrain_resolution" )]
	public static float TerrainResolution { get; set; } = 0.5f;

	/// <summary>
	/// Whether the terrain has a bordered wall surrounding it.
	/// </summary>
	[ConVar.Replicated( "terrain_border" )]
	public static bool TerrainBorder { get; set; } = false;

	/// <summary>
	/// Whether the terrain uses the altered noise generation.
	/// </summary>
	[ConVar.Replicated( "terrain_altered" )]
	public static bool AlteredTerrain { get; set; } = true;

	/// <summary>
	/// How much the terrain blobs are dilated by (only applicable to altered terrain).
	/// </summary>
	[ConVar.Replicated( "terrain_dilation" )]
	public static int DilationAmount { get; set; } = 1;

	public const int WindSteps = 3;

	// Grub Configuration
	public const float LowHealthThreshold = 30;

	public static string[] GrubNames => new[]
	{
		"Froggy",
		"Balls",
		"Boggy",
		"Spicy",
		"Hot",
		"Pinky",
		"Perky",
		"Gumby",
		"Dick",
		"Panini",
		"Wilson",
		"Winky",
		"Cammy",
		"Bakky",
		"Avoofo",
		"Gibby"
	};

	public static char[] TeamNames => new[]
	{
		'a',
		'b',
		'c',
		'd'
	};
}
