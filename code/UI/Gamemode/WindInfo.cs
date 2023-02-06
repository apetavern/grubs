using Grubs.Player;
using Grubs.States;
using Grubs.Utils.Event;

namespace Grubs.UI;

public class WindInfo : Panel
{
	private static BaseGamemode? Gamemode => BaseGamemode.Instance;

	public WindInfo()
	{
		StyleSheet.Load( "/UI/Stylesheets/WindInfo.scss" );
	}

	[GrubsEvent.TurnChanged.Client]
	private void TurnChanged( Team team )
	{
		if ( Gamemode is null )
			return;

		DeleteChildren( true );

		if ( Gamemode.WindSteps == 0 )
		{
			Add.Icon( "horizontal_rule" );
			return;
		}

		var direction = Gamemode.WindSteps < 0 ? "left" : "right";
		for ( var i = 0; i < Math.Abs( Gamemode.WindSteps ); i++ )
			Add.Icon( $"arrow_{direction}" );
	}
}
