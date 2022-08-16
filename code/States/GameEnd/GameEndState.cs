namespace Grubs.States;

public class GameEndState : BaseState
{
	protected override void Enter( bool forced, params object[] parameters )
	{
		base.Enter( forced, parameters );

		switch ( (GameResultType)parameters[0] )
		{
			case GameResultType.TeamWon:
				var teamWon = (int)parameters[1];
				break;
			case GameResultType.Abandoned:
				break;
			default:
				throw new ArgumentOutOfRangeException( nameof( parameters ) );
		}
	}
}
