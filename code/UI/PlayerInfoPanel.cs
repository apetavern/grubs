using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using TerryForm.UI.Elements;

namespace TerryForm.UI
{
	internal class PlayerInfoPanel : Panel
	{
		public PlayerInfoPanel()
		{
			StyleSheet.Load( "/Code/UI/PlayerInfoPanel.scss" );

			// Random background for stylistic purposes
			AddClass( $"background-{Rand.Int( 0, 3 )}" );

			Add.Image( "avatar:76561198128972602", "player-avatar" );
			Add.Label( "Player Name", "player-name" );

			AddChild<HealthBar>();
		}
	}
}
