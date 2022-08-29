using System.Globalization;
using Grubs.States;
using Grubs.Utils;

namespace Grubs.UI;

public class WaitingStatus : Panel
{
	private static WaitingState State => (GrubsGame.Current.CurrentState as WaitingState)!;

	private readonly Label _message;

	public WaitingStatus()
	{
		StyleSheet.Load( "/UI/Stylesheets/WaitingStatus.scss" );

		_message = Add.Label( "???", "message" );
	}

	public override void Tick()
	{
		base.Tick();

		if ( !State.IsStarting )
		{
			_message.Text = $"Waiting for players ({Client.All.Count}/{GameConfig.MaximumPlayers})";
			return;
		}

		var secondsTillStart = Math.Ceiling( State.TimeUntilStart ).ToString( CultureInfo.CurrentCulture );
		_message.Text = $"Starting in {secondsTillStart} seconds ({Client.All.Count}/{GameConfig.MaximumPlayers})";
	}
}
