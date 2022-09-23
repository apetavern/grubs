namespace Grubs.Utils;

/// <summary>
/// Utility class to handle array conversions in multiple dimensions.
/// </summary>
public static class Dimensions
{
	/// <summary>
	/// Converts a 2D point to a 1D array index.
	/// <remarks>The return value is not validated.</remarks>
	/// </summary>
	/// <param name="x">The x component of the point.</param>
	/// <param name="y">The y component of the point.</param>
	/// <param name="width">The width of the grid.</param>
	/// <returns>The converted 1D array index.</returns>
	public static int Convert2dTo1d( int x, int y, int width )
	{
		return x + width * y;
	}

	/// <summary>
	/// Converts a 1D array index to a 2D point.
	/// <remarks>The return value is not validated.</remarks>
	/// </summary>
	/// <param name="index">The index.</param>
	/// <param name="width">The width of the grid.</param>
	/// <returns>The converted 2D point.</returns>
	public static (int, int) Convert1dTo2d( int index, int width )
	{
		return (index % width, index / width);
	}
}
