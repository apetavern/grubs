using Grubs.States;

namespace Grubs.UI;

[UseTemplate]
public class EndScreen : Panel
{
	private static GameEndState? GameEndState => GrubsGame.Current.CurrentState as GameEndState;

	public Label TitleLabel { get; set; } = null!;
	public Label SubtitleLabel { get; set; } = null!;
	public Label ReplayLabel { get; set; } = null!;

	public override void Tick()
	{
		base.Tick();

		if ( GameEndState is null )
			return;

		ReplayLabel.Text = $"Returning to waiting state in {Math.Ceiling( GameEndState.TimeUntilRestart )} seconds";

		switch ( GameEndState.EndResult )
		{
			case GameResultType.Abandoned:
				TitleLabel.Text = "Game Abandoned";
				SubtitleLabel.Text = GameEndState.AbandonReason;

				TitleLabel.SetClass( "error", true );
				SubtitleLabel.SetClass( "error", true );
				break;
			case GameResultType.Draw:
				TitleLabel.Text = "Draw";
				SubtitleLabel.Text = string.Empty;
				break;
			case GameResultType.TeamWon:
				TitleLabel.Text = "Game Over";
				SubtitleLabel.Text = $"Team {GameEndState.WinningTeamName} has won!";

				TitleLabel.SetClass( $"team-{GameEndState.WinningTeamName}", true );
				SubtitleLabel.SetClass( $"team-{GameEndState.WinningTeamName}", true );
				break;
			default:
				throw new ArgumentOutOfRangeException();
		}
	}

	public void Quit()
	{
		Local.Client.Kick();
	}
}
