using Sandbox.UI;
using TerryForm.UI.World;

namespace TerryForm.UI
{
	internal class HudEntity : Sandbox.HudEntity<RootPanel>
	{
		public HudEntity()
		{
			if ( IsClient )
			{
				RootPanel.SetTemplate( "/Code/UI/HudEntity.html" );

				_ = new WormNametags();
			}
		}
	}
}
