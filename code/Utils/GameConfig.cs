namespace Grubs.Utils;

public static class GameConfig
{
	// Gameplay Configuration
	/// <summary>
	/// The amount of worms that will be spawned per player.
	/// </summary>
	[ConVar.Replicated( "worm_count" )]
	public static int WormCount { get; set; } = 10;
	/// <summary>
	/// The max time in seconds that a player has to make their turn.
	/// </summary>
	[ConVar.Replicated( "turn_duration" )]
	public static int TurnDuration { get; set; } = 60;
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
	/// Whether or not worms can damage their teammates.
	/// <remarks>This does not protect a worm from hurting itself.</remarks>
	/// </summary>
	[ConVar.Replicated( "friendly_fire" )]
	public static bool FriendlyFire { get; set; } = true;

	// Worm Configuration
	public static string[] WormNames => new[]
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

	[Net]
	public static int TeamIndex { get; set; } = 1;
}
