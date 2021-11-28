using Sandbox.UI;
using TerryForm.UI.World;

namespace TerryForm.UI
{
	public class HudEntity : Sandbox.HudEntity<RootPanel>
	{
		public static HudEntity Instance { get; set; }
		private bool ShouldReceiveInput { get; set; } = false;

		public HudEntity()
		{
			if ( IsClient )
			{
				Instance = this;
				RootPanel.SetTemplate( "/Code/UI/HudEntity.html" );

				_ = new WormNametags();
			}
		}

		public void ReceiveInput( bool receiveInput )
		{
			ShouldReceiveInput = receiveInput;
			RootPanel.SetClass( "ReceiveInput", ShouldReceiveInput );
		}
	}
}
