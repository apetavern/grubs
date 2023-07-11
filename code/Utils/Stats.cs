namespace Grubs;

public static class Stats
{
	/// <summary>
	/// Increment "gamemode-games-won" stat for winning client.
	/// </summary>
	/// <param name="client"></param>
	/// <param name="gamemode"></param>
	public static void IncrementGamesWon( string gamemode, IClient client )
	{
		if ( client.IsBot ) return;

		Sandbox.Services.Stats.Increment( client, $"{gamemode}-games-won", 1 );
	}

	/// <summary>
	/// Increment "gamemode-games-played" stat for all players.
	/// </summary>
	/// <param name="gamemode"></param>
	public static void IncrementGamesPlayed( string gamemode )
	{
		foreach ( var player in GamemodeSystem.Instance.Players.Where( p => !p.Client.IsBot ) )
		{
			Sandbox.Services.Stats.Increment( player.Client, $"{gamemode}-games-played", 1 );
		}
	}

	/// <summary>
	/// Increment "x-grubs-killed" and "weaponname-kills" stats for attacker.
	/// </summary>
	/// <param name="attacker"></param>
	/// <param name="victim"></param>
	public static void IncrementGrubsKilled( Player attacker, Player victim )
	{
		if ( attacker.Client.IsBot ) return;

		// General kill stat.
		string statIdent = "grubs-killed";
		if ( attacker == victim ) // Killed self.
			statIdent = "own-grubs-killed";
		else if ( victim.Client.IsBot ) // Killed a bot.
			statIdent = "bot-grubs-killed";

		Sandbox.Services.Stats.Increment( attacker.Client, statIdent, 1 );

		// Weapon specific kill stat.
		bool hasWeaponEquipped = attacker.Inventory.ActiveWeapon.IsValid();
		Weapon weapon = hasWeaponEquipped ? attacker.Inventory.ActiveWeapon : attacker.Inventory.LastActiveWeapon;
		if ( !weapon.IsValid() ) return;

		string weaponName = weapon.Name.ToLower().Replace( " ", "-" );
		Sandbox.Services.Stats.Increment( attacker.Client, $"{weaponName}-kills", 1 );
	}
}
