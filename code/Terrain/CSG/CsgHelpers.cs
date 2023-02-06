using System;
using System.Collections.Generic;
using System.Linq;

namespace Sandbox.Csg
{
    internal static class CsgHelpers
    {
        public const float UnitEpsilon = 9.5367431640625E-7f; // 0x35800000
        public const float DistanceEpsilon = 0.00390625f; // 0x3b800000

        [ThreadStatic]
        private static List<List<CsgHull.FaceCut>> _sFaceCutListPool;
        [ThreadStatic]
        private static List<List<CsgHull>> _sHullListPool;
        [ThreadStatic]
        private static List<HashSet<CsgHull>> _sHullSetPool;

        private const int PoolCapacity = 8;

        private static T RentContainer<T>( ref List<T> pool )
            where T : new()
        {
            if ( pool == null )
            {
                pool = new List<T>( Enumerable.Range( 0, PoolCapacity ).Select( x => new T() ) );
            }

            if ( pool.Count == 0 )
            {
                Log.Warning( $"Pool of List<{typeof(T)}> is empty!" );
                pool.Add( new T() );
            }

            var list = pool[pool.Count - 1];
            pool.RemoveAt( pool.Count - 1 );

            return list;
        }

        public static bool Equals( Vector3 a, Vector3 b )
        {
            // ReSharper disable CompareOfFloatsByEqualityOperator
            return a.x == b.x && a.y == b.y && a.z == b.z;
            // ReSharper enable CompareOfFloatsByEqualityOperator
        }

        public static void AssertAreEqual<T>( T a, T b, string message = null )
            where T : struct, IEquatable<T>
        {
            if ( !a.Equals( b ) )
            {
                throw new Exception( "Assert: AreEqual " + message );
            }
        }

        private static void ReturnContainer<T>( List<T> pool, T list )
        {
            if ( pool.Count >= PoolCapacity ) return;

            pool.Add( list );
        }

        public static List<CsgHull.FaceCut> RentFaceCutList()
        {
            var list = RentContainer( ref _sFaceCutListPool );

            CsgHelpers.AssertAreEqual( 0, list.Count );

            return list;
        }

        public static void Return( List<CsgHull.FaceCut> list )
        {
            list.Clear();

            ReturnContainer( _sFaceCutListPool, list );
        }

        public static List<CsgHull> RentHullList()
        {
            var list = RentContainer( ref _sHullListPool );

            CsgHelpers.AssertAreEqual( 0, list.Count );

            return list;
        }

        public static void Return( List<CsgHull> list )
        {
            list.Clear();

            ReturnContainer( _sHullListPool, list );
        }

        public static HashSet<CsgHull> RentHullSet()
        {
            var list = RentContainer( ref _sHullSetPool );

            CsgHelpers.AssertAreEqual( 0, list.Count );

            return list;
        }

        public static void Return( HashSet<CsgHull> set )
        {
            set.Clear();

            ReturnContainer( _sHullSetPool, set );
        }

        public static Vector3 GetTangent( this Vector3 normal )
        {
            var absX = Math.Abs(normal.x);
            var absY = Math.Abs(normal.y);
            var absZ = Math.Abs(normal.z);

            return Vector3.Cross(normal, absX <= absY && absX <= absZ
                ? new Vector3(1f, 0f, 0f) : absY <= absZ
                    ? new Vector3(0f, 1f, 0f)
                    : new Vector3(0f, 0f, 1f));
        }

        private static int NextPowerOfTwo( int value )
        {
            var po2 = 1;
            while ( po2 < value ) po2 <<= 1;
            return po2;
        }

        public static void EnsureCapacity<T>( ref T[] array, int minSize )
            where T : struct
        {
            if (array != null && array.Length >= minSize) return;

            var oldArray = array;

            array = new T[NextPowerOfTwo(minSize)];

            if (oldArray != null)
            {
                Array.Copy(oldArray, 0, array, 0, oldArray.Length);
            }
        }

        public static float Cross( Vector2 a, Vector2 b )
        {
            return a.x * b.y - a.y * b.x;
        }

        public static void Flip( this List<CsgHull.FaceCut> faceCuts,
            in CsgPlane.Helper oldHelper, in CsgPlane.Helper newHelper )
        {
            for ( var i = 0; i < faceCuts.Count; i++ )
            {
                faceCuts[i] = -oldHelper.Transform( faceCuts[i], newHelper );
            }
        }

        public static bool IsDegenerate( this List<CsgHull.FaceCut> faceCuts )
        {
            if ( faceCuts == null )
            {
                return false;
            }

            if ( faceCuts.Count >= 3 ) return false;

            foreach ( var cut in faceCuts )
            {
                if ( float.IsInfinity( cut.Min ) ) return false;
                if ( float.IsInfinity( cut.Max ) ) return false;
            }

            return true;
        }

        public static bool Contains( this List<CsgHull.FaceCut> faceCuts, Vector2 pos )
        {
            foreach ( var faceCut in faceCuts )
            {
                if ( Dot( faceCut.Normal, pos ) - faceCut.Distance < -DistanceEpsilon )
                {
                    return false;
                }
            }

            return true;
        }

        public static Vector2 GetAveragePos( this List<CsgHull.FaceCut> faceCuts )
        {
            if ( faceCuts.Count == 0 )
            {
                return Vector2.Zero;
            }

            var avgPos = Vector2.Zero;

            foreach ( var faceCut in faceCuts )
            {
                avgPos += faceCut.GetPos( faceCut.Mid );
            }

            return avgPos / faceCuts.Count;
        }

        public static float Dot( Vector2 a, Vector2 b )
        {
            return a.x * b.x + a.y * b.y;
        }

        public static bool TryMerge( this List<CsgHull.FaceCut> faceCuts, List<CsgHull.FaceCut> otherCuts )
        {
            Assert.True( faceCuts != otherCuts );

            if ( faceCuts.Count < 3 || otherCuts.Count < 3 ) return false;

            var newCuts = RentFaceCutList();

            try
            {
                newCuts.AddRange( faceCuts );

                var dividingCutIndex = -1;

                foreach ( var otherCut in otherCuts )
                {
                    if ( float.IsInfinity( otherCut.Min ) || float.IsInfinity( otherCut.Max ) )
                    {
                        return false;
                    }

                    var canAdd = true;

                    for ( var i = 0; i < faceCuts.Count; i++ )
                    {
                        var thisCut = faceCuts[i];

                        if ( thisCut.ApproxEquals( otherCut ) )
                        {
                            thisCut.Min = Math.Min( thisCut.Min, otherCut.Min );
                            thisCut.Max = Math.Max( thisCut.Max, otherCut.Max );

                            newCuts[i] = thisCut;
                            canAdd = false;

                            break;
                        }

                        if ( thisCut.ApproxEquals( -otherCut ) )
                        {
                            canAdd = false;
                            dividingCutIndex = i;
                            break;
                        }
                    }

                    if ( canAdd )
                    {
                        newCuts.Add( otherCut );
                    }
                }

                if ( dividingCutIndex == -1 )
                {
                    return false;
                }

                newCuts.RemoveAt( dividingCutIndex );

                // Convexity check

                foreach ( var faceCut in faceCuts )
                {
                    var pos = faceCut.GetPos( faceCut.Min );

                    foreach ( var newCut in newCuts )
                    {
                        if ( newCut.GetSign( pos ) < 0 ) return false;
                    }
                }

                foreach ( var faceCut in otherCuts )
                {
                    var pos = faceCut.GetPos( faceCut.Min );

                    foreach ( var newCut in newCuts )
                    {
                        if ( newCut.GetSign( pos ) < 0 ) return false;
                    }
                }

                // Output

                faceCuts.Clear();
                faceCuts.AddRange( newCuts );

                return true;
            }
            finally
            {
                Return( newCuts );
            }
        }

        public static bool Split( this List<CsgHull.FaceCut> faceCuts, CsgHull.FaceCut splitCut, List<CsgHull.FaceCut> outNegative = null )
        {
            outNegative?.Clear();

            if ( splitCut.ExcludesNone || splitCut.ExcludesAll )
            {
                return false;
            }

            var outPositive = RentFaceCutList();

            var newCut = new CsgHull.FaceCut( splitCut.Normal, splitCut.Distance,
                float.NegativeInfinity, float.PositiveInfinity );

            try
            {
                foreach ( var faceCut in faceCuts )
                {
                    var cross = Cross( splitCut.Normal, faceCut.Normal );
                    var dot = Dot( splitCut.Normal, faceCut.Normal );

                    if ( Math.Abs( cross ) <= UnitEpsilon )
                    {
                        // Edge case: parallel cuts

                        if ( faceCut.Distance * dot - splitCut.Distance < DistanceEpsilon )
                        {
                            // splitCut is pointing away from faceCut,
                            // so faceCut is negative

                            if ( dot < 0f && splitCut.Distance * dot - faceCut.Distance < DistanceEpsilon )
                            {
                                // faceCut is also pointing away from splitCut,
                                // so the whole face must be negative

                                outNegative?.Clear();
                                return false;
                            }

                            outNegative?.Add( faceCut );
                            continue;
                        }

                        if ( splitCut.Distance * dot - faceCut.Distance < DistanceEpsilon )
                        {
                            // faceCut is pointing away from splitCut,
                            // so splitCut is redundant

                            outNegative?.Clear();
                            return false;
                        }

                        // Otherwise the two cuts are pointing towards each other

                        outPositive.Add( faceCut );
                        continue;
                    }

                    // Not parallel, so check for intersection

                    var proj0 = (faceCut.Distance - splitCut.Distance * dot) / cross;
                    var proj1 = (splitCut.Distance - faceCut.Distance * dot) / -cross;

                    var posFaceCut = faceCut;
                    var negFaceCut = faceCut;

                    if ( cross > 0f )
                    {
                        splitCut.Min = Math.Max( splitCut.Min, proj0 );
                        newCut.Min = Math.Max( newCut.Min, proj0 );
                        posFaceCut.Max = Math.Min( faceCut.Max, proj1 );
                        negFaceCut.Min = Math.Max( faceCut.Min, proj1 );
                    }
                    else
                    {
                        splitCut.Max = Math.Min( splitCut.Max, proj0 );
                        newCut.Max = Math.Min( newCut.Max, proj0 );
                        posFaceCut.Min = Math.Max( faceCut.Min, proj1 );
                        negFaceCut.Max = Math.Min( faceCut.Max, proj1 );
                    }

                    if ( splitCut.Max - splitCut.Min < DistanceEpsilon )
                    {
                        // splitCut must be fully outside the face

                        outNegative?.Clear();
                        return false;
                    }

                    if ( posFaceCut.Max - posFaceCut.Min >= DistanceEpsilon )
                    {
                        outPositive.Add( posFaceCut );
                    }

                    if ( negFaceCut.Max - negFaceCut.Min >= DistanceEpsilon )
                    {
                        outNegative?.Add( negFaceCut );
                    }
                }

                outPositive.Add( newCut );
                outNegative?.Add( -newCut );

                if ( outPositive.IsDegenerate() || outNegative.IsDegenerate() )
                {
                    outNegative?.Clear();
                    return false;
                }

                faceCuts.Clear();
                faceCuts.AddRange( outPositive );

                return true;
            }
            finally
            {
                Return( outPositive );
            }
        }

        public static float GetArea( this List<Vector2> vertices )
        {
            if ( vertices.Count < 3 ) return 0f;

            var area = 0f;
            var prev = vertices[^1];

            foreach ( var next in vertices )
            {
                area += (next.x - prev.x) * (next.y + prev.y);
                prev = next;
            }

            return area * 0.5f;
        }

        private static int Mod( int k, int n ) => (k %= n) < 0 ? k + n : k;

        private static int FindEarIndex( List<Vector2> vertices, int offset )
        {
            Assert.True( vertices.Count - offset >= 3 );

            if ( vertices.Count - offset == 3 )
            {
                return offset;
            }

            var count = vertices.Count - offset;

            for ( var i = offset; i < vertices.Count; i++ )
            {
                var prev = vertices.GetWithinRange( offset, i - 1 );
                var curr = vertices[i];
                var next = vertices.GetWithinRange( offset, i + 1 );

                if ( Cross( next - curr, curr - prev ) <= 0f )
                {
                    continue;
                }

                var anyIntersections = false;

                for ( var j = 1; j < count - 1; j++ )
                {
                    var a = vertices.GetWithinRange( offset, i + j );
                    var b = vertices.GetWithinRange( offset, i + j + 1 );

                    if ( LinesIntersect( prev, next, a, b ) )
                    {
                        anyIntersections = true;
                        break;
                    }
                }

                if ( anyIntersections )
                {
                    continue;
                }

                return i;
            }

            throw new Exception();
        }

        private static bool Ccw( Vector2 a, Vector2 b, Vector2 c )
        {
            return (c.y - a.y) * (b.x - a.x) > (b.y - a.y) * (c.x - a.x);
        }

        public static bool LinesIntersect( Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2 )
        {
            var a = a2 - a1;
            var b = b2 - b1;

            if ( Math.Abs( Cross( a, b ) ) < UnitEpsilon ) return false;

            return Ccw( a1, b1, b2 ) != Ccw( a2, b1, b2 ) && Ccw( a1, a2, b1 ) != Ccw( a1, a2, b2 );
        }

        private static T GetWithinRange<T>( this IList<T> list, int offset, int index )
        {
            return list[offset + Mod( index - offset, list.Count - offset )];
        }

        private static T GetWithinRange<T>( this IList<T> list, int offset, int count, int index )
        {
            return list[offset + Mod( index - offset, count )];
        }

        public static void MakeConvex( List<Vector2> vertices, List<int> outVertCounts )
        {
            Assert.True( vertices.Count >= 3 );

            if ( vertices.GetArea() < 0f )
            {
                vertices.Reverse();
            }

            // Ear clipping, can speed this up
            // Also this only outputs triangles for now

            var offset = 0;

            while ( vertices.Count - offset > 3 )
            {
                var earIndex = FindEarIndex( vertices, offset );

                var a = vertices.GetWithinRange( offset, earIndex - 1 );
                var b = vertices[earIndex];
                var c = vertices.GetWithinRange( offset, earIndex + 1 );

                vertices.RemoveAt( earIndex );

                vertices.Insert( offset++, a );
                vertices.Insert( offset++, b );
                vertices.Insert( offset++, c );

                outVertCounts.Add( 3 );
            }

            AssertAreEqual( 3, vertices.Count - offset );

            outVertCounts.Add( 3 );
        }
    }
}
