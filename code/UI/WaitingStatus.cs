using System.Globalization;
using Grubs.States;
using Grubs.Utils;

namespace Grubs.UI;

public sealed class WaitingStatus : Panel
{
	private static WaitingState? State => BaseState.Instance as WaitingState;

	private readonly Label _message;

	public WaitingStatus()
	{
		StyleSheet.Load( "/UI/Stylesheets/WaitingStatus.scss" );

		_message = Add.Label( "???", "message" );
	}

	public override void Tick()
	{
		base.Tick();

		if ( State is null )
			return;

		if ( !State.IsStarting )
		{
			_message.Text = $"Waiting for players ({Game.Clients.Count}/{GameConfig.MaximumPlayers})";
			return;
		}

		var secondsTillStart = Math.Ceiling( State.TimeUntilStart ).ToString( CultureInfo.CurrentCulture );
		_message.Text = $"Starting in {secondsTillStart} seconds ({Game.Clients.Count}/{GameConfig.MaximumPlayers})";
	}
}
