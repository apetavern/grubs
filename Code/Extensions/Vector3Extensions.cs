namespace Grubs.Extensions;

public static class Vector3Extensions
{
	public static Vector2 ToVector2( this Vector3 vec )
	{
		return new Vector2( vec.x, vec.z );
	}
}
