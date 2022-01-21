using Sandbox.UI;

namespace Grubs.UI.Menu
{

	[UseTemplate]
	public partial class MenuPanel : Panel
	{
		public NavBar NavBar { get; set; }
		public Panel WindowPanel { get; set; }
		public WormPreviewScene WormPreviewScene { get; set; }
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

		public override void OnHotloaded()
		{
			base.OnHotloaded();

			WormPreviewScene.Build();
		}

		protected override void PostTemplateApplied()
		{
			WormPreviewScene.Build();
		}

		private void DefineMenuButtonListeners()
		{
			NavBar.PlayButton.AddEventListener( "onclick", () =>
			{
				if ( ActiveWindow == Windows.PLAY ) return;

				ActiveWindow = Windows.PLAY;
				ClearWindowPanel();

				Panel menuHolder = WindowPanel.Add.Panel( "menu-holder" );
				menuHolder.AddChild( new PlayerList() );
				menuHolder.AddChild( new StartButton() );
				Panel sceneHolder = WindowPanel.Add.Panel( "menu-holder" );
				sceneHolder.AddChild( WormPreviewScene );
				WormPreviewScene.Build();

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
