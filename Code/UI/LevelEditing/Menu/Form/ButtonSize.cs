namespace Grubs.UI;

public enum ButtonSize
{
	Small,
	Medium,
	Large,
	ExtraLarge,
}

public static class ButtonSizeExtensions
{
	public static string GetClassName( this ButtonSize buttonSize )
	{
		return buttonSize switch
		{
			ButtonSize.Small => "size-sm",
			ButtonSize.Medium => "size-md",
			ButtonSize.Large => "size-lg",
			ButtonSize.ExtraLarge => "size-xl",
			_ => "size-md"
		};
	}
}
