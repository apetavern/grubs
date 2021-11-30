using Sandbox;
using Sandbox.UI;
using System.Collections.Generic;

namespace TerryForm.UI
{
	public class PlayerInfoContainer : Panel
	{
		private Dictionary<Client, PlayerInfoPanel> playerInfoPanels = new();

		public PlayerInfoContainer() { }

		private void Update()
		{
			foreach ( var client in Client.All )
			{
				if ( playerInfoPanels.ContainsKey( client ) )
					continue;

				var playerInfoPanel = new PlayerInfoPanel( client );
				var index = playerInfoPanels.Count % 4;

				playerInfoPanel.AddClass( $"background-{index} {(client.Pawn as Pawn.Player).ActiveWorm.GetTeamClass()} my-turn" );
				playerInfoPanel.Parent = this;
				playerInfoPanels.Add( client, playerInfoPanel );
			}
		}

		TimeSince timeSinceLastUpdate = 0;

		public override void Tick()
		{
			base.Tick();

			if ( timeSinceLastUpdate > 1 )
			{
				Update();
			}
		}
	}
}
