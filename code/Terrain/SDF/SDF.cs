using Sandbox;
using System.Linq;
using Sandbox.UI;
using System;
using System.Collections.Generic;

namespace Grubs.Terrain
{
	public abstract partial class SDF : BaseNetworkable
	{
		public enum MergeType
		{
			Add,
			Subtract,
			SmoothAdd,
			SmoothSubtract
		}
		[Net] public MergeType Type { get; set; }
		public abstract float GetDistance( Vector2 point );
	}

	public struct MathSDF
	{
		public static float Add( float a, float b ) => MathF.Min( a, b );
		public static float Subtract( float a, float b ) => MathF.Max( a, -b );
		public static float Operate( float a, float b, bool boolean ) => boolean ? Add( a, b ) : Subtract( a, b );
		public static float SmoothOperate( float a, float b, bool boolean, float k ) => boolean ? SmoothAdd( a, b, k ) : SmoothSubtract( a, b, k );

		public const float Smoothing = (1 << Quadtree.ExtentShifts >> Quadtree.Levels) * 0.5f;

		public static float Operate( float a, float b, SDF.MergeType type )
		{
			switch ( type )
			{
				case SDF.MergeType.Subtract:
					return Subtract( a, b );
				case SDF.MergeType.SmoothAdd:
					return SmoothAdd( a, b, 25f );
				case SDF.MergeType.SmoothSubtract:
					return SmoothSubtract( a, b, 25f );
				default:
					return Add( a, b );
			}
		}

		public static float SmoothAdd( float a, float b, float k )
		{
			float h = Clamp( 0.5f + 0.5f * (b - a) / k, 0f, 1f );
			return Blend( b, a, h ) - k * h * (1f - h);
		}

		public static float SmoothSubtract( float a, float b, float k )
		{
			float h = Clamp( 0.5f - 0.5f * -(b + a) / k, 0f, 1f );
			return Blend( -b, a, h ) + k * h * (1f - h);
		}

		public static float Blend( float a, float b, float t ) => a * (1f - t) + b * t;
		private static float Clamp( float a, float min, float max )
		{
			if ( a < min )
				return min;
			if ( a > max )
				return max;
			return a;
		}

		public static float RoundCone( Vector2 from, Vector2 to, float r1, float r2, Vector2 point )
		{
			// sampling independent computations (only depend on shape)
			Vector2 ba = to - from;
			float l2 = (float)Vector2.GetDot( ba, ba );
			float rr = r1 - r2;
			float a2 = l2 - rr * rr;
			float il2 = 1f / l2;

			// sampling dependant computations
			Vector2 pa = point - from;
			float y = (float)Vector2.GetDot( pa, ba );
			float z = y - l2;
			float x2 = Dot2( pa * l2 - ba * y );
			float y2 = y * y * l2;
			float z2 = z * z * l2;

			// single square root!
			float k = MathF.Sign( rr ) * rr * rr * x2;
			if ( MathF.Sign( z ) * a2 * z2 > k ) return MathF.Sqrt( x2 + z2 ) * il2 - r2;
			if ( MathF.Sign( y ) * a2 * y2 < k ) return MathF.Sqrt( x2 + y2 ) * il2 - r1;
			return (MathF.Sqrt( x2 * a2 * il2 ) + y * rr) * il2 - r1;
		}

		private static float Dot2( Vector2 v ) => (float)Vector2.GetDot( v, v );
	}
}
