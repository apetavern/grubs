using Sandbox;
using Sandbox.UI;
using Grubs.UI.World;

namespace Grubs.UI
{
	public partial class PlayingHudEntity : Sandbox.HudEntity<RootPanel>
	{
		public static PlayingHudEntity Instance { get; set; }
		private bool ShouldReceiveInput { get; set; } = false;

		public PlayingHudEntity()
		{
			if ( IsClient )
			{
				Instance = this;
				RootPanel.SetTemplate( "/UI/PlayingHudEntity.html" );

				_ = new WormNametags();
			}
		}

		public void ReceiveInput( bool receiveInput )
		{
			ShouldReceiveInput = receiveInput;
			RootPanel.SetClass( "ReceiveInput", ShouldReceiveInput );
		}

		[ClientRpc]
		public static void UpdateWind( float wind )
		{
			WindInfoPanel.Instance?.UpdateWind( wind );
		}
	}
}
