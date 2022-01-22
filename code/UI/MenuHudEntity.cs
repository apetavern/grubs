using Sandbox;
using Sandbox.UI;

namespace Grubs.UI
{
	public partial class MenuHudEntity : HudEntity<RootPanel>
	{
		public static MenuHudEntity Instance { get; set; }

		public MenuHudEntity()
		{
			if ( IsClient )
			{
				Instance = this;
				RootPanel.SetTemplate( "/UI/MenuHudEntity.html" );
			}
		}
	}
}
