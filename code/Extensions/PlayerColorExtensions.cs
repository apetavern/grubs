using Grubs.Systems.Pawn;

namespace Grubs.Extensions;

public static class PlayerColorExtensions
{
	public static Color Color( this PlayerColor color )
	{
		return color switch
		{
			PlayerColor.Lavender => Parse( "#714197" ),
			PlayerColor.Cerulean => Parse( "#3C5083" ),
			PlayerColor.Sage => Parse( "#2F795F" ),
			PlayerColor.Khaki => Parse( "#818449" ),
			PlayerColor.Copper => Parse( "#A86244" ),
			PlayerColor.Coral => Parse( "#D88470" ),
			PlayerColor.Teal => Parse( "#62928C" ),
			PlayerColor.Mauve => Parse( "#9B8195" ),
			_ => throw new ArgumentOutOfRangeException( nameof(color), color, null )
		};
	}

	private static Color Parse( string hex )
	{
		return global::Color.Parse( hex ) ?? global::Color.White;
	}
}
