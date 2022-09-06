using System.Globalization;
using Grubs.Player;
using Grubs.States;
using Grubs.Utils;

namespace Grubs.UI;

public class TurnTime : Panel
{
	private readonly Label _timeLeft;

	public TurnTime()
	{
		StyleSheet.Load( "/UI/Stylesheets/TurnTime.scss" );

		_timeLeft = Add.Label( GameConfig.TurnDuration.ToString(), "time-left" );
	}

	public override void Tick()
	{
		base.Tick();

		if ( GrubsGame.Current.CurrentState is not BaseGamemode )
			return;

		SetClass( "hidden", GrubsGame.Current.CurrentGamemode.UsedTurn );
		if ( HasClass( "hidden" ) )
			return;

		_timeLeft.Text = Math.Floor( GrubsGame.Current.CurrentGamemode.TimeUntilTurnEnd ).ToString( CultureInfo.CurrentCulture );

		// TODO: Event for when turn changes and update there
		foreach ( var teamName in GameConfig.TeamNames )
			SetClass( $"team-{teamName}", TeamManager.Instance.CurrentTeam.TeamName == teamName.ToString() );
	}
}
