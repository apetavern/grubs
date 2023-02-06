namespace Grubs.States;

/// <summary>
/// The end state to games. This will display the result of the completed gamemode.
/// </summary>
public sealed partial class GameEndState : BaseState
{
	/// <summary>
	/// The end result of the game.
	/// </summary>
	[Net]
	public GameResultType EndResult { get; private set; }

	/// <summary>
	/// The list of clients that have won the game.
	/// <remarks>This will only be populated when <see cref="EndResult"/> is <see cref="GameResultType.TeamWon"/>.</remarks>
	/// </summary>
	[Net]
	public IList<IClient> WinningClients { get; private set; }

	/// <summary>
	/// The name of the team that won.
	/// </summary>
	[Net]
	public string WinningTeamName { get; private set; }

	/// <summary>
	/// The reason that the game was abandoned.
	/// <remarks>This will only be populated when <see cref="EndResult"/> is <see cref="GameResultType.Abandoned"/>.</remarks>
	/// </summary>
	[Net]
	public string AbandonReason { get; private set; }

	/// <summary>
	/// The time until the game will go back to the <see cref="WaitingState"/>.
	/// </summary>
	[Net]
	public TimeUntil TimeUntilRestart { get; private set; }

	protected override void Enter( bool forced, params object[] parameters )
	{
		base.Enter( forced, parameters );

		if ( !Game.IsServer )
			return;

		if ( forced )
			parameters = new object[] { GameResultType.Abandoned, "Forced" };

		EndResult = (GameResultType)parameters[0];
		switch ( EndResult )
		{
			case GameResultType.TeamWon:
				WinningTeamName = (string)parameters[1];
				var winningClients = (IList<IClient>)parameters[2];
				foreach ( var winningClient in winningClients )
					WinningClients.Add( winningClient );

				break;
			case GameResultType.Draw:
				break;
			case GameResultType.Abandoned:
				AbandonReason = (string)parameters[1];
				break;
			default:
				throw new ArgumentOutOfRangeException( nameof( parameters ) );
		}

		TimeUntilRestart = 20;
	}

	public override void Tick()
	{
		base.Tick();

		if ( !Game.IsServer || TimeUntilRestart > 0 )
			return;

		SwitchStateTo<WaitingState>();
	}
}
