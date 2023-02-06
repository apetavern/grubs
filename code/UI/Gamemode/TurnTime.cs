using System.Globalization;
using Grubs.Player;
using Grubs.States;
using Grubs.Utils;
using Grubs.Utils.Event;

namespace Grubs.UI;

public sealed class TurnTime : Panel
{
	private static BaseGamemode? Gamemode => BaseGamemode.Instance;

	private readonly Label _timeLeft;

	public TurnTime()
	{
		StyleSheet.Load( "/UI/Stylesheets/TurnTime.scss" );

		_timeLeft = Add.Label( GameConfig.TurnDuration.ToString(), "time-left" );
	}

	public override void Tick()
	{
		base.Tick();

		if ( Gamemode is null )
			return;

		SetClass( "hidden", Gamemode.UsedTurn );
		if ( HasClass( "hidden" ) )
			return;

		_timeLeft.Text = Math.Floor( Gamemode.TimeUntilTurnEnd ).ToString( CultureInfo.CurrentCulture );
	}

	[GrubsEvent.TurnChanged.Client]
	private void TurnChanged( Team newTeam )
	{
		foreach ( var teamName in GameConfig.TeamNames )
			SetClass( $"team-{teamName}", TeamManager.Instance.CurrentTeam.TeamName == teamName.ToString() );
	}
}
