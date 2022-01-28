using Sandbox.UI;

namespace Grubs.UI.Menu
{
	[UseTemplate]
	public partial class NavBar : Panel
	{
		public Button PlayButton { get; set; }
		public Button CustomizeButton { get; set; }
		public Button OptionsButton { get; set; }

		public NavBar()
		{
			PlayButton.AddEventListener( "onclick", () =>
			{
				DeactivateButtons();
				PlayButton.AddClass( "active" );
			} );
			CustomizeButton.AddEventListener( "onclick", () =>
			{
				DeactivateButtons();
				CustomizeButton.AddClass( "active" );
			} );
			OptionsButton.AddEventListener( "onclick", () =>
			{
				DeactivateButtons();
				OptionsButton.AddClass( "active" );
			} );
		}

		public void DeactivateButtons()
		{
			PlayButton.SetClass( "active", false );
			CustomizeButton.SetClass( "active", false );
			OptionsButton.SetClass( "active", false );
		}
	}
}
