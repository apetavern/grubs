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

	public static Color DarkColor( this PlayerColor color )
	{
		return color switch
		{
			PlayerColor.Lavender => Parse( "#3F2454" ),
			PlayerColor.Cerulean => Parse( "#213967" ),
			PlayerColor.Sage => Parse( "#28554D" ),
			PlayerColor.Khaki => Parse( "#4B593F" ),
			PlayerColor.Copper => Parse( "#67483E" ),
			PlayerColor.Coral => Parse( "#8B4D4D" ),
			PlayerColor.Teal => Parse( "#2D5753" ),
			PlayerColor.Mauve => Parse( "#614A58" ),
			_ => throw new ArgumentOutOfRangeException( nameof(color), color, null )
		};
	}

	private static Color Parse( string hex )
	{
		return global::Color.Parse( hex ) ?? global::Color.White;
	}
}
