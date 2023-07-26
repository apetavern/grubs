namespace Grubs;

public static class IClientExtensions
{
	/// <summary>
	/// Gets the value of a leaderboard entry for a given client.
	/// If the client is invalid, a bot, or does not have an entry, 0 is returned.
	/// </summary>
	/// <param name="leaderboard">Ident of the leaderboard to fetch.</param>
	public static async Task<double> GetLeaderboardEntry( this IClient client, string leaderboard )
	{
		if ( !client.IsValid() || client.IsBot ) return 0f;

		var steamId = client.SteamId;
		var board = Sandbox.Services.Leaderboards.Get( leaderboard );
		board.TargetSteamId = steamId;
		await board.Refresh();

		var entry = board.Entries.Where( e => e.SteamId == steamId ).FirstOrDefault();
		if ( entry.SteamId != steamId ) return 0f;

		return entry.Value;
	}

	/// <summary>
	/// Get all stats (even ones with no entries) for this client.
	/// If the client is invalid or a bot, null is returned.
	/// </summary>
	public static Sandbox.Services.Stats.PlayerStats GetPlayerStats( this IClient client )
	{
		if ( !client.IsValid() || client.IsBot ) return null;

		return Sandbox.Services.Stats.GetPlayerStats( "apetavern.grubs", client.SteamId );
	}
}
