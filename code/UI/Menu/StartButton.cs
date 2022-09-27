using Grubs.Utils;

namespace Grubs.UI.Menu;

[UseTemplate]
public class StartButton : Panel
{
	public override void Tick()
	{
		SetClass( "disabled", Client.All.Count < GameConfig.MinimumPlayers );
	}

	protected override void OnClick( MousePanelEvent e )
	{
		base.OnClick( e );

/*		if ( Client.All.Count < Game.Instance.StateHandler.LobbyCount ||
			Client.All.Count < GameConfig.MinimumPlayersToStart ) return;*/

		StartGame();

	}

	[ConCmd.Server]
	public static void StartGame()
	{
		Log.Info( "Start game" );
		// Game.Instance.StateHandler.ChangeState( new PlayingState() );
	}
}
