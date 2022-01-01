using System;

namespace Grubs.Terrain
{

	public struct SDF
	{
		public static float Sphere( Vector3 center, float radius, Vector3 point )
		{
			return center.Distance( point ) - radius;
		}

		public static float Lerp( float a, float b, float t )
		{
			return t * a + (1f - t) * b;
		}

		public static float Clamp( float v, float min, float max )
		{
			if ( v > max )
				return max;
			if ( v < min )
				return min;
			return v;
		}

		public static float SmoothMin( float a, float b, float k = 32 )
		{
			float res = MathF.Exp( -k * a ) + MathF.Exp( -k * b );
			return -MathF.Log( MathF.Max( 0.0001f, res ) ) / k;
		}
	}
}
