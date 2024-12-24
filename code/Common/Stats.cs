using Grubs.Pawn;
using Grubs.Systems.Pawn;

namespace Grubs.Common;

public static class Stats
{
	public const string GrubsKilled = "grubs-killed";
	public const string OwnGrubsKilled = "own-grubs-killed";
	public const string BotGrubsKilled = "bot-grubs-killed";

	public static string GamesPlayed( string gamemode ) => $"{gamemode.ToLower()}-games-played";
	public static string GamesWon( string gamemode ) => $"{gamemode.ToLower()}-games-won";

	/// <summary>
	/// Increment "gamemode-games-won" stat.
	/// </summary>
	[Broadcast]
	public static void IncrementGamesWon( string gamemode )
	{
		if ( Game.IsEditor )
			return;

		Sandbox.Services.Stats.Increment( GamesWon( gamemode ), 1 );
		Sandbox.Services.Stats.Flush();
	}

	/// <summary>
	/// Increment "gamemode-games-played" stat.
	/// </summary>
	[Broadcast]
	public static void IncrementGamesPlayed( string gamemode )
	{
		if ( Game.IsEditor )
			return;

		Sandbox.Services.Stats.Increment( GamesPlayed( gamemode ), 1 );
		Sandbox.Services.Stats.Flush();
	}

	/// <summary>
	/// Increment "x-grubs-killed" stat.
	/// </summary>
	[Broadcast]
	public static void IncrementGrubsKilled( Guid victimPlayerGuid )
	{
		if ( Game.IsEditor )
			return;

		var victim = Game.ActiveScene.Directory.FindComponentByGuid( victimPlayerGuid ) as Player;
		string statIdent = (victim?.IsProxy ?? true) ? GrubsKilled : OwnGrubsKilled;

		Sandbox.Services.Stats.Increment( statIdent, 1 );
		Sandbox.Services.Stats.Flush();
	}
}
