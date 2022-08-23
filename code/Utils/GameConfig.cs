namespace Grubs.Utils;

public static class GameConfig
{
	// Gameplay Configuration
	[ConVar.Replicated( "worm_count" )]
	public static int WormCount { get; set; } = 10;
	[ConVar.Replicated( "turn_duration" )]
	public static int TurnDuration { get; set; } = 60;
	[ConVar.Replicated( "minimum_players" )]
	public static int MinimumPlayers { get; set; } = 2;
	[ConVar.Replicated( "maximum_players" )]
	public static int MaximumPlayers { get; set; } = 4;
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
