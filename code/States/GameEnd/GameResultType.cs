namespace Grubs.States;

/// <summary>
/// Represents an ending state to a gamemode.
/// </summary>
public enum GameResultType
{
	/// <summary>
	/// A team has won the game.
	/// </summary>
	TeamWon,
	/// <summary>
	/// Multiple teams have won the game.
	/// </summary>
	Draw,
	/// <summary>
	/// Something occurred that prevented the game from continuing.
	/// </summary>
	Abandoned
}
