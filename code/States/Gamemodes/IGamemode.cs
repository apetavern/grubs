namespace Grubs.States;

public interface IGamemode
{
	IList<Client> Participants { get; }
	int TeamsTurn { get; }
	bool UsedTurn { get; }
	TimeUntil TimeUntilTurnEnd { get; }

	void UseTurn();
	void NextTurn();
}
