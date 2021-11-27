using Sandbox;
using Sandbox.UI;
using TerryForm.States;

namespace TerryForm.UI
{
	[UseTemplate]
	public class PlayerInfoPanel : Panel
	{
		public Label PlayerNameLabel { get; set; }
		public Image PlayerAvatarImage { get; set; }

		public PlayerInfoPanel( Client client )
		{
			StyleSheet.Load( "/Code/UI/PlayerInfoPanel.scss" );

			PlayerNameLabel.Text = client.Name;
			PlayerAvatarImage.SetTexture( $"avatar:{client.PlayerId}" );

			BindClass( "my-turn", () =>
			{
				var game = Game.Instance;
				if ( game == null ) return false;

				var state = game.StateHandler.State;
				if ( state == null ) return false;

				if ( state is PlayingState { Turn: { ActivePlayer: Pawn.Player player } } )
				{
					return player.Client == client;
				}

				return false;
			} );
		}
	}
}
