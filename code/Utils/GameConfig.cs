using Grubs.Terrain;

namespace Grubs.Utils;

public static class GameConfig
{
	/// <summary>
	/// The grubs gamemode to play.
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
	/// The amount of grubs that will be spawned per player.
	/// </summary>
	[ConVar.Replicated( "grub_count" )]
	public static int GrubCount { get; set; } = 4;
	/// <summary>
	/// The max time in seconds that a player has to make their turn.
	/// </summary>
	[ConVar.Replicated( "turn_duration" )]
	public static int TurnDuration { get; set; } = 60;
	/// <summary>
	/// Whether or not grubs can damage their teammates.
	/// <remarks>This does not protect a grub from hurting itself.</remarks>
	/// </summary>
	[ConVar.Replicated( "friendly_fire" )]
	public static bool FriendlyFire { get; set; } = true;
	/// <summary>
	/// The type of material to be used for the terrain.
	/// </summary>
	[ConVar.Replicated( "terrain_type" )]
	public static string TerrainType { get; set; } = "sand";

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
