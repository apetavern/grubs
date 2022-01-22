using Sandbox;
using System.Linq;
using Sandbox.UI;
using System;
using System.Collections.Generic;

namespace Grubs.Terrain
{
	public abstract partial class SDF : Entity
	{
		public enum MergeType
		{
			Add,
			Subtract,
			Intersect,
			SmoothAdd,
			SmoothSubtract,
			SmoothIntersect,
		}

		public SDF() { Transmit = TransmitType.Always; }

		[Net] public MergeType Type { get; set; }
		[Net] public new IList<SDF> Children { get; set; } = new List<SDF>();
		public abstract float GetDistance( Vector2 point );
		public float FullDistance( Vector2 point )
		{
			float value = GetDistance( point );
			foreach ( SDF sdf in Children )
			{
				float childValue = sdf.GetDistance( point );
				value = MathSDF.Operate( value, childValue, sdf.Type );
			}
			return value;
		}
		public void Add( SDF otherSDF )
		{
			Children.Add( otherSDF );
		}
	}

	public struct MathSDF
	{
		public static float Operate( float a, float b, SDF.MergeType type )
		{
			switch ( type )
			{
				case SDF.MergeType.SmoothAdd:
					return SmoothAdd( a, b );
				case SDF.MergeType.SmoothIntersect:
					return SmoothIntersect( a, b );
				case SDF.MergeType.SmoothSubtract:
					return SmoothSubtract( a, b );
				case SDF.MergeType.Add:
					return Add( a, b );
				case SDF.MergeType.Intersect:
					return Intersect( a, b );
				default:
					return Subtract( a, b );
			}
		}

		public static float Add( float a, float b ) => MathF.Min( a, b );
		public static float Subtract( float a, float b ) => MathF.Max( a, -b );
		public static float Intersect( float a, float b ) => MathF.Max( a, b );

		public const float Smoothing = 1 << Quadtree.ExtentShifts >> (Quadtree.Levels - 2);
		public static float SmoothAdd( float a, float b, float k = Smoothing )
		{
			float h = Clamp( 0.5f + 0.5f * (b - a) / k, 0f, 1f );
			return Blend( b, a, h ) - k * h * (1f - h);
		}

		public static float SmoothSubtract( float a, float b, float k = Smoothing )
		{
			float h = Clamp( 0.5f - 0.5f * -(b + a) / k, 0f, 1f );
			return Blend( -b, a, h ) + k * h * (1f - h);
		}
		public static float SmoothIntersect( float a, float b, float k = Smoothing )
		{
			float h = Clamp( 0.5f - 0.5f * (b - a) / k, 0f, 1f );
			return Blend( b, a, h ) + k * h * (1f - h);
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
			// Sampling independent computations.
			Vector2 ba = to - from;
			float l2 = (float)Vector2.GetDot( ba, ba );
			float rr = r1 - r2;
			float a2 = l2 - rr * rr;
			float il2 = 1f / l2;

			// Sampling dependent computations.
			Vector2 pa = point - from;
			float y = (float)Vector2.GetDot( pa, ba );
			float z = y - l2;
			float x2 = Dot2( pa * l2 - ba * y );
			float y2 = y * y * l2;
			float z2 = z * z * l2;

			// Single Square Root.
			float k = MathF.Sign( rr ) * rr * rr * x2;
			if ( MathF.Sign( z ) * a2 * z2 > k ) return MathF.Sqrt( x2 + z2 ) * il2 - r2;
			if ( MathF.Sign( y ) * a2 * y2 < k ) return MathF.Sqrt( x2 + y2 ) * il2 - r1;
			return (MathF.Sqrt( x2 * a2 * il2 ) + y * rr) * il2 - r1;
		}

		private static float Dot2( Vector2 v ) => (float)Vector2.GetDot( v, v );
	}
}
