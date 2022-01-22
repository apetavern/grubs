using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System.Linq;

namespace Grubs.UI.Menu
{
	public partial class PlayerEntry : Panel
	{
		public Client Client { get; set; }
		public Image Avatar { get; set; }
		public Label PlayerName { get; set; }
		public bool Loaded { get; set; }

		public PlayerEntry( Client client )
		{
			Client = client;
			Avatar = Add.Image( $"avatar:{client.PlayerId}", "avatar" );
			PlayerName = Add.Label( client.Name, "name" );
		}

		public override void Tick()
		{
			base.Tick();

			if ( !Client.IsValid() ) return;

			SetClass( "loaded", Loaded );
		}
	}

	[UseTemplate]
	public partial class PlayerList : Panel
	{
		Panel PlayersContainer { get; set; }

		public string PlayerCount => $"{Client.All.Count}";
		public string LobbyCount => $"{ Game.Instance?.StateHandler.LobbyCount }";

		public PlayerList()
		{
			StyleSheet.Load( "/UI/Menu/PlayerList.scss" );
		}

		public override void Tick()
		{
			foreach ( var panel in PlayersContainer.Children.OfType<PlayerEntry>() )
			{
				if ( panel.Client.IsValid() ) continue;
				panel.Delete();
			}

			foreach ( var client in Client.All )
			{
				if ( PlayersContainer.Children.OfType<PlayerEntry>().Any( panel => panel.Client == client ) ) continue;
				PlayersContainer.AddChild( new PlayerEntry( client ) );
			}
		}
	}
}
