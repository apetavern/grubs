using Grubs.States;
using Grubs.Utils;
using Sandbox;
using Sandbox.UI;

namespace Grubs.UI.Menu
{
	[UseTemplate]
	public partial class StartButton : Panel
	{
		public StartButton()
		{
			StyleSheet.Load( "/UI/Menu/StartButton.scss" );
		}

		public override void Tick()
		{
			SetClass( "disabled", Client.All.Count < GameConfig.MinimumPlayersToStart );
		}

		protected override void OnClick( MousePanelEvent e )
		{
			base.OnClick( e );

			if ( Client.All.Count < Game.Instance.StateHandler.LobbyCount ||
				Client.All.Count < GameConfig.MinimumPlayersToStart ) return;

			StartGame();

		}

		[ServerCmd]
		public static void StartGame()
		{
			Game.Instance.StateHandler.ChangeState( new PlayingState() );
		}
	}
}
