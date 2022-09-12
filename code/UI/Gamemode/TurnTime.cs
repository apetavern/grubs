using System.Globalization;
using Grubs.Player;
using Grubs.States;
using Grubs.Utils;
using Grubs.Utils.Event;

namespace Grubs.UI;

public sealed class TurnTime : Panel
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
	}

	[GrubsEvent.TurnChanged.Client]
	private void TurnChanged( Team newTeam )
	{
		foreach ( var teamName in GameConfig.TeamNames )
			SetClass( $"team-{teamName}", TeamManager.Instance.CurrentTeam.TeamName == teamName.ToString() );
	}
}
