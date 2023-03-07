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
	public static int TurnDuration { get; set; } = 45;

	/// <summary>
	/// The percent chance that a Weapon Crate will spawn every turn.
	/// </summary>
	[ConVar.Replicated( "crate_weapon_chance" )]
	public static int WeaponCrateChancePerTurn { get; set; } = 5;
	
	/// <summary>
	/// The percent chance that a Tools Crate will spawn every turn.
	/// </summary>
	[ConVar.Replicated( "crate_tool_chance" )]
	public static int ToolsCrateChancePerTurn { get; set; } = 5;

	/// <summary>
	/// The percent chance that a Health Crate will spawn every turn.
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

	public enum TerrainType
	{
		Generated = 0,
		Texture = 1,
	}

	public enum TerrainTexture
	{
		Grubs = 0,
		TestLevel = 1,
		Battlefield = 2,
		Cavern = 3,
		Bunkers = 4,
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
