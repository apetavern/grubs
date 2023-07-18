namespace Grubs;

public static class Stats
{
	public const string GrubsKilled = "grubs-killed";
	public const string OwnGrubsKilled = "own-grubs-killed";
	public const string BotGrubsKilled = "bot-grubs-killed";

	public static string GamesPlayed( string gamemode ) => $"{gamemode}-games-played";
	public static string GamesWon( string gamemode ) => $"{gamemode}-games-won";
	public static string WeaponKills( string weapon )
	{
		weapon = weapon.Trim().ToLower().Replace( " ", "-" );
		return $"weapon-{weapon}-kills";
	}

	/// <summary>
	/// Increment "gamemode-games-won" stat for winning client.
	/// </summary>
	/// <param name="client"></param>
	/// <param name="gamemode"></param>
	public static void IncrementGamesWon( string gamemode, IClient client )
	{
		if ( !client.IsValid() || client.IsBot ) return;

		Sandbox.Services.Stats.Increment( client, GamesWon( gamemode ), 1 );
	}

	/// <summary>
	/// Increment "gamemode-games-played" stat for all players.
	/// </summary>
	/// <param name="gamemode"></param>
	public static void IncrementGamesPlayed( string gamemode )
	{
		foreach ( var player in GamemodeSystem.Instance.Players.Where( p => p.Client.IsValid() && !p.Client.IsBot ) )
		{
			Sandbox.Services.Stats.Increment( player.Client, GamesPlayed( gamemode ), 1 );
		}
	}

	/// <summary>
	/// Increment "x-grubs-killed" and "weaponname-kills" stats for attacker.
	/// </summary>
	/// <param name="attacker"></param>
	/// <param name="victim"></param>
	public static void IncrementGrubsKilled( Player attacker, Player victim )
	{
		if ( !attacker.Client.IsValid() || attacker.Client.IsBot ) return;

		// General kill stat.
		string statIdent = GrubsKilled;
		if ( attacker == victim ) // Killed self.
			statIdent = OwnGrubsKilled;
		else if ( victim.Client.IsBot ) // Killed a bot.
			statIdent = BotGrubsKilled;

		Sandbox.Services.Stats.Increment( attacker.Client, statIdent, 1 );

		// Weapon specific kill stat.
		bool hasWeaponEquipped = attacker.Inventory.ActiveWeapon.IsValid();
		Weapon weapon = hasWeaponEquipped ? attacker.Inventory.ActiveWeapon : attacker.Inventory.LastActiveWeapon;
		if ( !weapon.IsValid() ) return;

		Sandbox.Services.Stats.Increment( attacker.Client, WeaponKills( weapon.Name ), 1 );
	}
}
