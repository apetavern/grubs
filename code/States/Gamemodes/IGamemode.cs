namespace Grubs.States;

public interface IGamemode
{
	bool UsedTurn { get; }
	TimeUntil TimeUntilTurnEnd { get; }

	void UseTurn();
	void NextTurn();
}
