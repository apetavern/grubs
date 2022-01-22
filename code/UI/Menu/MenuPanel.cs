using Sandbox.UI;

namespace Grubs.UI.Menu
{

	[UseTemplate]
	public partial class MenuPanel : Panel
	{
		public NavBar NavBar { get; set; }
		public Panel WindowPanel { get; set; }
		private Windows ActiveWindow { get; set; } = Windows.PLAY;

		private enum Windows
		{
			PLAY,
			CUSTOMIZE,
			OPTIONS
		};

		public MenuPanel()
		{
			DefineMenuButtonListeners();
		}

		private void DefineMenuButtonListeners()
		{
			NavBar.PlayButton.AddEventListener( "onclick", () =>
			{
				if ( ActiveWindow == Windows.PLAY ) return;

				ActiveWindow = Windows.PLAY;
				ClearWindowPanel();

				WindowPanel.AddChild( new PlayPanel() );

			} );

			NavBar.CustomizeButton.AddEventListener( "onclick", () =>
			{
				if ( ActiveWindow == Windows.CUSTOMIZE ) return;

				ActiveWindow = Windows.CUSTOMIZE;
				ClearWindowPanel();

				WindowPanel.AddChild( new CustomizePanel() );

			} );

			NavBar.OptionsButton.AddEventListener( "onclick", () =>
			{
				if ( ActiveWindow == Windows.OPTIONS ) return;

				ActiveWindow = Windows.OPTIONS;
				ClearWindowPanel();
			} );
		}

		private void ClearWindowPanel()
		{
			foreach ( var panel in WindowPanel.Children )
			{
				panel.Delete();
			}
		}
	}
}
