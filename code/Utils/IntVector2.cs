namespace Grubs.Utils;

/// <summary>
/// An integer based <see cref="Vector2"/>.
/// </summary>
public readonly struct IntVector2
{
	/// <summary>
	/// The x component
	/// </summary>
	public readonly int X;
	/// <summary>
	/// The y component
	/// </summary>
	public readonly int Y;

	public IntVector2( int x, int y )
	{
		X = x;
		Y = y;
	}
}
