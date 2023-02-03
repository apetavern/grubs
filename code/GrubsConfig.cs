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
	/// The maximum amount of players that can be in the game.
	/// </summary>
	[ConVar.Replicated( "maximum_players" )]
	public static int MaximumPlayers { get; set; } = 4;

	/// <summary>
	/// The amount of <see cref="Grub"/>s that will be spawned per team.
	/// </summary>
	[ConVar.Replicated( "grub_count" )]
	public static int GrubCount { get; set; } = 1;

	/// <summary>
	/// The max time in seconds that a player has to make their turn.
	/// </summary>
	[ConVar.Replicated( "turn_duration" )]
	public static int TurnDuration { get; set; } = 30;

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
}
