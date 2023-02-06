using System;
using System.Collections.Generic;

namespace Sandbox.Csg
{
	public readonly struct CsgPlane : IEquatable<CsgPlane>
	{
		public static CsgPlane operator -( CsgPlane plane )
		{
			return new CsgPlane( -plane.Normal, -plane.Distance );
		}

		public static CsgPlane operator +( CsgPlane plane, float offset )
		{
			return new CsgPlane( plane.Normal, plane.Distance + offset );
		}

		public static CsgPlane operator -( CsgPlane plane, float offset )
		{
			return new CsgPlane( plane.Normal, plane.Distance - offset );
		}

		public readonly Vector3 Normal;
		public readonly float Distance;

		public CsgPlane( Vector3 normalDir, Vector3 position )
		{
			Normal = normalDir.Normal;
			Distance = Vector3.Dot( Normal, position );
		}

		public CsgPlane( Vector3 normal, float distance )
		{
			Normal = normal;
			Distance = distance;
		}

		public Helper GetHelper()
		{
			return new Helper( this );
		}

		public CsgPlane Transform( in Matrix matrix )
		{
			var basis = GetHelper();
			var position = matrix.Transform( basis.Origin );
			var p1 = matrix.Transform( basis.Origin + basis.Tu );
			var p2 = matrix.Transform( basis.Origin + basis.Tv );

			return new CsgPlane( Vector3.Cross( p2 - position, p1 - position ), position );
		}

		public int GetSign( Vector3 pos )
		{
			var dot = Vector3.Dot( pos, Normal ) - Distance;

			return dot > CsgHelpers.DistanceEpsilon ? 1 : dot < -CsgHelpers.DistanceEpsilon ? -1 : 0;
		}

		public bool Equals( CsgPlane other )
		{
			return CsgHelpers.Equals( Normal, other.Normal ) && Distance.Equals( other.Distance );
		}

		public bool ApproxEquals( CsgPlane other )
		{
			return
				Math.Abs( 1f - Vector3.Dot( Normal, other.Normal ) ) < CsgHelpers.UnitEpsilon &&
				Math.Abs( Distance - other.Distance ) <= CsgHelpers.DistanceEpsilon;
		}

		public override bool Equals( object obj )
		{
			return obj is CsgPlane other && Equals( other );
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (Normal.GetHashCode() * 397) ^ Distance.GetHashCode();
			}
		}

		public override string ToString()
		{
			return $"{{ Normal: {Normal}, Distance: {Distance} }}";
		}

		public readonly struct Helper
		{
			public readonly Vector3 Origin;
			public readonly float Offset;
			public readonly Vector3 Normal;
			public readonly Vector3 Tu;
			public readonly Vector3 Tv;

			public Helper( in CsgPlane plane )
			{
				Normal = plane.Normal;
				Offset = plane.Distance;

				Tu = Normal.GetTangent().Normal;
				Tv = Vector3.Cross( Tu, Normal ).Normal;

				Origin = Normal * plane.Distance;
			}

			public Vector2 Project( Vector3 pos )
			{
				return new Vector2( Vector3.Dot( pos, Tu ), Vector3.Dot( pos, Tv ) );
			}

			public CsgHull.FaceCut GetCut( CsgPlane cutPlane )
			{
				if ( 1f - Math.Abs( Vector3.Dot( Normal, cutPlane.Normal ) ) <= CsgHelpers.UnitEpsilon )
				{
					// If this cut completely excludes the original plane, return a FaceCut that also excludes everything

					var dot = Vector3.Dot( Normal, cutPlane.Normal );

					return dot * Offset - cutPlane.Distance > CsgHelpers.DistanceEpsilon ? CsgHull.FaceCut.ExcludeNone : CsgHull.FaceCut.ExcludeAll;
				}

				var cutTangent = Vector3.Cross( Normal, cutPlane.Normal );
				var cutNormal = Vector3.Cross( cutTangent, Normal );

				cutNormal = cutNormal.Normal;

				var cutNormal2 = new Vector2(
					Vector3.Dot( cutNormal, Tu ),
					Vector3.Dot( cutNormal, Tv ) );

				cutNormal2 = cutNormal2.Normal;

				var t = Vector3.Dot( cutPlane.Normal * cutPlane.Distance - Origin, cutPlane.Normal )
						/ Vector3.Dot( cutPlane.Normal, cutNormal );

				return new CsgHull.FaceCut( cutNormal2, t, float.NegativeInfinity, float.PositiveInfinity );
			}

			public Vector3 GetPoint( Vector2 uv )
			{
				return Origin + Tu * uv.x + Tv * uv.y;
			}

			public Vector3 GetPoint( CsgHull.FaceCut cut )
			{
				return GetPoint( cut, cut.Mid );
			}

			public Vector3 GetPoint( CsgHull.FaceCut cut, float along )
			{
				var pos = cut.Normal * cut.Distance + new Vector2( -cut.Normal.y, cut.Normal.x ) * along;

				return Origin + Tu * pos.x + Tv * pos.y;
			}

			public Vector3 GetAveragePos( List<CsgHull.FaceCut> faceCuts )
			{
				if ( faceCuts.Count == 0 )
				{
					return Normal * Offset;
				}

				var avgPos = faceCuts.GetAveragePos();

				return Normal * Offset + Tu * avgPos.x + Tv * avgPos.y;
			}

			public CsgHull.FaceCut Transform( CsgHull.FaceCut cut, in Helper newHelper, in Matrix? matrix = null )
			{
				if ( float.IsNegativeInfinity( cut.Min ) || float.IsPositiveInfinity( cut.Max ) )
				{
					throw new NotImplementedException();
				}

				var oldTangent = Tu * -cut.Normal.y + Tv * cut.Normal.x;
				var newTangent = oldTangent;

				var minPos3 = GetPoint( cut, cut.Min );
				var maxPos3 = GetPoint( cut, cut.Max );

				if ( matrix is { } mat )
				{
					newTangent = mat.TransformNormal( oldTangent ).Normal;

					minPos3 = mat.Transform( minPos3 );
					maxPos3 = mat.Transform( maxPos3 );
				}

				var midPos3 = (minPos3 + maxPos3) * 0.5f;

				var normal = new Vector2(
					Vector3.Dot( newHelper.Tv, newTangent ),
					-Vector3.Dot( newHelper.Tu, newTangent ) );

				var midPos2 = new Vector2(
					Vector3.Dot( newHelper.Tu, midPos3 ),
					Vector3.Dot( newHelper.Tv, midPos3 ) );

				var min = Vector3.Dot( minPos3, newTangent );
				var max = Vector3.Dot( maxPos3, newTangent );

				return new CsgHull.FaceCut( normal, Vector3.Dot( normal, midPos2 ),
					Math.Min( min, max ), Math.Max( min, max ) );
			}
		}
	}
}
