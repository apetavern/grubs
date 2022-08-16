namespace Grubs.Utils;

public static class GameConfig
{
	// Gameplay Configuration
	public static int WormCount { get; set; } = 1;
	public static int MinimumPlayers { get; } = 2;
	public static int MaximumPlayers { get; } = 4;
	public static int TurnDuration { get; } = 60;

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
