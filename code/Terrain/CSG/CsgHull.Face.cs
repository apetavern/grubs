using System;
using System.Collections.Generic;

namespace Sandbox.Csg
{
    partial class CsgHull
    {
        public partial struct Face : IEquatable<Face>
        {
            public CsgPlane Plane;
            public List<FaceCut> FaceCuts;
            public List<SubFace> SubFaces;

            public override string ToString()
            {
                return $"{{ Plane: {Plane}, FaceCuts: {FaceCuts?.Count} }}";
            }

            public Face Clone()
            {
                var copy = new Face
                {
                    Plane = Plane,
                    FaceCuts = new List<FaceCut>( FaceCuts ),
                    SubFaces = new List<SubFace>( SubFaces.Count )
                };

                foreach ( var subFace in SubFaces )
                {
                    copy.SubFaces.Add( subFace.Clone() );
                }

                return copy;
            }

            public Face CloneFlipped( CsgHull neighbor )
            {
                var copy = new Face
                {
                    Plane = -Plane,
                    FaceCuts = new List<FaceCut>( FaceCuts ),
                    SubFaces = new List<SubFace>( SubFaces.Count )
                };

                var thisHelper = Plane.GetHelper();
                var flipHelper = copy.Plane.GetHelper();

                copy.FaceCuts.Flip( thisHelper, flipHelper );

                foreach ( var subFace in SubFaces )
                {
                    var subFaceCopy = subFace.Clone();

                    subFaceCopy.FaceCuts.Flip( thisHelper, flipHelper );
                    subFaceCopy.Neighbor = neighbor;

                    copy.SubFaces.Add( subFaceCopy );
                }
                
                return copy;
            }

            public void RemoveSubFacesInside( List<FaceCut> faceCuts )
            {
                var negCuts = CsgHelpers.RentFaceCutList();

                try
                {
                    for ( var i = SubFaces.Count - 1; i >= 0; i-- )
                    {
                        var thisSubFace = SubFaces[i];

                        foreach ( var otherFaceCut in faceCuts )
                        {
                            if ( !thisSubFace.FaceCuts.Split( otherFaceCut, negCuts ) )
                            {
                                continue;
                            }
                            
                            SubFaces.Add( thisSubFace with { FaceCuts = new List<FaceCut>( negCuts ) } );
                        }
                        
                        if ( faceCuts.Contains( thisSubFace.FaceCuts.GetAveragePos() ) )
                        {
                            SubFaces.RemoveAt( i );
                        }
                    }
                }
                finally
                {
                    CsgHelpers.Return( negCuts );
                }
            }

            public bool Equals( Face other )
            {
                return Plane.Equals(other.Plane);
            }

            public override bool Equals( object obj )
            {
                return obj is Face other && Equals(other);
            }

            public override int GetHashCode()
            {
                return Plane.GetHashCode();
            }
        }

        public partial struct SubFace
        {
            public List<FaceCut> FaceCuts;
            public CsgHull Neighbor;
            public CsgMaterial Material;

            public SubFace Clone()
            {
                return this with { FaceCuts = new List<FaceCut>( FaceCuts ) };
            }
        }

        public partial struct FaceCut : IComparable<FaceCut>, IEquatable<FaceCut>
        {
            public static Comparison<FaceCut> Comparer { get; } = ( x, y ) => Math.Sign(x.Angle - y.Angle);

            public static FaceCut ExcludeAll => new FaceCut(new Vector2(-1f, 0f),
                float.PositiveInfinity, float.NegativeInfinity, float.PositiveInfinity);

            public static FaceCut ExcludeNone => new FaceCut(new Vector2(1f, 0f),
                float.NegativeInfinity, float.NegativeInfinity, float.PositiveInfinity);

            public static FaceCut operator -( FaceCut cut )
            {
                return new FaceCut(-cut.Normal, -cut.Distance, -cut.Max, -cut.Min);
            }

            public static FaceCut operator +( FaceCut cut, float offset )
            {
                return new FaceCut(cut.Normal, cut.Distance + offset, float.NegativeInfinity, float.PositiveInfinity);
            }

            public static FaceCut operator -( FaceCut cut, float offset )
            {
                return new FaceCut(cut.Normal, cut.Distance - offset, float.NegativeInfinity, float.PositiveInfinity);
            }

            public readonly Vector2 Normal;
            public readonly float Angle;
            public readonly float Distance;

            public float Min;
            public float Max;

            public bool ExcludesAll => float.IsPositiveInfinity(Distance);
            public bool ExcludesNone => float.IsNegativeInfinity(Distance);

            public float Mid => !float.IsNegativeInfinity( Min ) && !float.IsPositiveInfinity( Max )
                ? (Min + Max) * 0.5f
                : !float.IsNegativeInfinity( Min )
                    ? Min + 1f
                    : !float.IsNegativeInfinity( Max )
                        ? Max - 1f
                        : 0f;

            public FaceCut( Vector2 normal, float distance, float min, float max ) => (Normal, Angle, Distance, Min, Max) = (normal, MathF.Atan2(normal.y, normal.x), distance, min, max);
            
            public int CompareTo( FaceCut other )
            {
                return Angle.CompareTo(other.Angle);
            }

            public Vector2 GetPos( float along )
            {
                return Normal * Distance + new Vector2( -Normal.y, Normal.x ) * along;
            }

            public int GetSign( Vector2 pos )
            {
                var dot = CsgHelpers.Dot( pos, Normal ) - Distance;

                return dot > CsgHelpers.DistanceEpsilon ? 1 : dot < -CsgHelpers.DistanceEpsilon ? -1 : 0;
            }

            public override string ToString()
            {
                return $"{{ Normal: {Normal}, Distance: {Distance} }}";
            }

            public bool Equals( FaceCut other )
            {
                return Normal.Equals(other.Normal) && Distance.Equals(other.Distance);
            }

            public bool ApproxEquals( FaceCut other )
            {
                return
                    Math.Abs( 1f - CsgHelpers.Dot( Normal, other.Normal ) ) < CsgHelpers.UnitEpsilon &&
                    Math.Abs( Distance - other.Distance ) <= CsgHelpers.DistanceEpsilon;
            }

            public override bool Equals( object obj )
            {
                return obj is FaceCut other && Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (Normal.GetHashCode() * 397) ^ Distance.GetHashCode();
                }
            }
        }
    }
}
