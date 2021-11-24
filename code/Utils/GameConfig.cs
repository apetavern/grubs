namespace TerryForm.Utils
{
	public static class GameConfig
	{
		public static int WormCount { get; set; } = 1;
		public static int TurnDurationSeconds { get; set; } = 45;
		public static int MinimumPlayersToStart { get; set; } = 2;

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
			"Panini"
		};
	}
}
