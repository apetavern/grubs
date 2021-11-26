using System.Collections.Generic;
using TerryForm.Weapons;

namespace TerryForm.Utils
{
	public static class GameConfig
	{
		public static int WormCount { get; set; } = 2;
		public static int TurnDurationSeconds { get; set; } = 45;
		public static int TurnTimeRemainingAfterFired { get; set; } = 5;
		public static int MinimumPlayersToStart { get; set; } = 2;
		public static Dictionary<string, int> AmmoDefaults { get; set; } = new()
		{
			{ "BaseballBat", 2 },
			{ "Bazooka", -1 },
			{ "Grenade", -1 },
			{ "Railgun", 0 },
			{ "Shotgun", 2 }
		};
	}
}
