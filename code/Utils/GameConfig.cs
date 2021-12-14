using System.Collections.Generic;

namespace TerryForm.Utils
{
	public static class GameConfig
	{
		public static int WormCount { get; set; } = 2;
		public static int TurnDurationSeconds { get; set; } = 45;
		public static int TurnTimeRemainingAfterFired { get; set; } = 5;
		public static int MinimumPlayersToStart { get; set; } = 2;

		// Used to spawn a random crate using the library system
		public static string[] CrateTypes => new[]
		{
			"crate_tools",
			"crate_weapons",
			"crate_health",
		};

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

		// Classname, weapon quantity.
		public static Dictionary<string, int> LoadoutDefaults = new()
		{
			{ "BaseballBat", -1 },
			{ "Bazooka", -1 },
			{ "Grenade", -1 },
			{ "Railgun", -1 },
			{ "Shotgun", -1 },
			{ "Dynamite", 1 },
			{ "Uzi", -1 },
			{ "Boomer", -1 },
		};
	}
}
