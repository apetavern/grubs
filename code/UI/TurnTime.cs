using System.Globalization;
using Grubs.States;
using Grubs.Utils;

namespace Grubs.UI;

public class TurnTime : Panel
{
	private static PlayState State => GrubsGame.Current.CurrentState as PlayState;

	private readonly Label _timeLeft;

	public TurnTime()
	{
		StyleSheet.Load( "/UI/Stylesheets/TurnTime.scss" );

		_timeLeft = Add.Label( GameConfig.TurnDuration.ToString(), "time-left" );
	}

	public override void Tick()
	{
		base.Tick();

		if ( State is null )
			return;

		_timeLeft.Text = Math.Floor( State.TimeUntilTurnEnd ).ToString( CultureInfo.CurrentCulture );
		
		// TODO: Event for when turn changes and update there
		foreach ( var teamName in GameConfig.TeamNames )
			SetClass( $"team-{teamName}", GameConfig.TeamNames[State.TeamsTurn-1] == teamName );
	}
}
