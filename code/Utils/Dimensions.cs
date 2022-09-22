namespace Grubs.Utils;

public static class Dimensions
{
	public static int Convert2dTo1d( int x, int y, int width )
	{
		return x + width * y;
	}

	public static (int, int) Convert1dTo2d( int index, int width )
	{
		return (index % width, index / width);
	}
}
