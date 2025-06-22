namespace Grubs.UI;

public enum ButtonColor
{
	Blue,
	Yellow,
	White,
}

public static class ButtonColorExtensions
{
	public static string GetBackgroundColorClassName( this ButtonColor buttonColor )
	{
		return buttonColor switch
		{
			ButtonColor.Blue => "bg-color-blue",
			ButtonColor.Yellow => "bg-color-yellow",
			ButtonColor.White => "bg-color-white",
			_ => "bg-color-blue",
		};
	}

	public static string GetColorClassName( this ButtonColor buttonColor )
	{
		return buttonColor switch
		{
			ButtonColor.Blue => "color-blue",
			ButtonColor.Yellow => "color-yellow",
			ButtonColor.White => "color-white",
			_ => "color-blue",
		};
	}

	public static string GetBorderColorClassName( this ButtonColor buttonColor )
	{
		return buttonColor switch
		{
			ButtonColor.Blue => "border-blue",
			ButtonColor.Yellow => "border-yellow",
			ButtonColor.White => "border-white",
			_ => "border-blue",
		};
	}
}
