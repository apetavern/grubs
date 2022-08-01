using Grubs.UI.World;

namespace Grubs.UI;

[UseTemplate]
public class Hud : RootPanel
{
	public Hud()
	{
		if ( Host.IsClient )
		{
			_ = new WormNametags();
		}
	}
}
