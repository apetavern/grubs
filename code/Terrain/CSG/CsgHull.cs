using System;
using System.Collections.Generic;

namespace Sandbox.Csg
{
    public partial class CsgHull
    {
        private readonly List<Face> _faces = new ();
        private readonly List<Vector3> _vertices = new ();
        private readonly Dictionary<CsgHull, CsgPlane> _neighbors = new ();

        public static int NextIndex { get; set; }

        public int Index { get; }

        public CsgMaterial Material { get; set; }

        internal (int X, int Y, int Z) GridCoord { get; set; }
        internal CsgSolid.GridCell GridCell { get; set; }

        public bool IsEmpty { get; private set; }
        public bool IsFinite => !float.IsPositiveInfinity( Volume );

        public IReadOnlyList<Face> Faces => _faces;

        private bool _vertexPropertiesInvalid = true;
        private bool _neighborsInvalid = true;

        private Vector3 _vertexAverage;
        private BBox _vertexBBox;
        private float _volume;

        public Vector3 VertexAverage
        {
            get
            {
                UpdateVertices();
                return _vertexAverage;
            }
        }

        public BBox VertexBounds
        {
            get
            {
                UpdateVertices();
                return _vertexBBox;
            }
        }

        public float Volume
        {
            get
            {
                UpdateVertices();
                return _volume;
            }
        }

        public CsgHull()
        {
            Index = NextIndex++;
        }

        public void InvalidateNeighbors()
        {
            _neighborsInvalid = true;

            GridCell?.InvalidateConnectivity();
        }

        public void InvalidateMesh()
        {
            GridCell?.InvalidateMesh();
        }
        
        public CsgHull Clone()
        {
            var copy = new CsgHull
            {
                Material = Material,
                IsEmpty = IsEmpty
            };

            foreach ( var face in _faces )
            {
                copy.AddFace( face.Clone() );
            }

            copy.InvalidateMesh();
            copy.InvalidateCollision();

            return copy;
        }

        private static bool HasSeparatingFace( List<Face> faces, List<Vector3> verts )
        {
            foreach ( var face in faces )
            {
                var anyPositive = false;

                foreach ( var vertex in verts )
                {
                    if ( face.Plane.GetSign( vertex ) >= 0 )
                    {
                        anyPositive = true;
                        break;
                    }
                }

                if ( !anyPositive ) return true;
            }

            return false;
        }

        public bool IsTouching( CsgHull other )
        {
            other.UpdateVertices();

            if ( HasSeparatingFace( _faces, other._vertices ) )
            {
                return false;
            }

            UpdateVertices();

            return !HasSeparatingFace( other._faces, _vertices );
        }

        public int GetNeighbors( List<CsgHull> outNeighbors )
        {
            UpdateNeighbors();

            outNeighbors.AddRange( _neighbors.Keys );

            return _neighbors.Count;
        }

        public bool TryMerge( CsgHull other )
        {
            Assert.False( this == other );
            Assert.NotNull( GridCell );

            // We can merge if:
            // * The two hulls are touching on a plane
            // * They have the same material
            // * They're in the same grid cell
            // * All sub-faces on the touching plane are neighbors of each other
            // * When ignoring the touching plane, all vertices are within the
            //     union of both plane lists

            if ( other.Material != Material ) return false;
            if ( other.GridCell != GridCell ) return false;

            UpdateNeighbors();

            if ( !_neighbors.TryGetValue( other, out var plane ) )
            {
                return false;
            }

            Assert.True( TryGetFace( plane, out var thisFace ) );
            Assert.True( other.TryGetFace( -thisFace.Plane, out var otherFace ) );

            // Make sure all sub faces are neighbors of each other

            foreach ( var subFace in thisFace.SubFaces )
            {
                if ( subFace.Neighbor != other )
                {
                    return false;
                }
            }

            foreach ( var subFace in otherFace.SubFaces )
            {
                if ( subFace.Neighbor != this )
                {
                    return false;
                }
            }

            // Make sure no vertices in the other hull are excluded by
            // this hull's faces (excluding shared plane)

            other.UpdateVertices();

            foreach ( var face in _faces )
            {
                if ( face.Plane.Equals( thisFace.Plane ) ) continue;

                foreach ( var vertex in other._vertices )
                {
                    if ( face.Plane.GetSign( vertex ) < 0 )
                    {
                        return false;
                    }
                }
            }

            // And the reverse

            foreach ( var face in other._faces )
            {
                if ( face.Plane.Equals( otherFace.Plane ) ) continue;

                foreach ( var vertex in _vertices )
                {
                    if ( face.Plane.GetSign( vertex ) < 0 )
                    {
                        return false;
                    }
                }
            }

            // It's safe to merge!

            RemoveFace( thisFace );
            other.RemoveFace( otherFace );

            foreach ( var face in other._faces )
            {
                if ( !TryGetFace( face.Plane, out var adjacentFace ) )
                {
                    foreach ( var subFace in face.SubFaces )
                    {
                        Assert.True( subFace.Neighbor != this );
                    }

                    AddFace( face.Clone() );
                    continue;
                }

                // Face is coplanar and adjacent to one already in this hull

                Assert.True( adjacentFace.FaceCuts.TryMerge( face.FaceCuts ) );

                // Merge in sub faces

                foreach ( var subFace in face.SubFaces )
                {
                    adjacentFace.SubFaces.Add( subFace.Clone() );
                }

                _neighborsInvalid = true;
            }

            other.SetEmpty( this );

            return true;
        }

        public int MergeSubFaces()
        {
            var mergeCount = 0;

            foreach ( var face in _faces )
            {
                for ( var i = 0; i < face.SubFaces.Count; i++ )
                {
                    var subFace = face.SubFaces[i];

                    bool merged;

                    do
                    {
                        merged = false;

                        for ( var j = i + 1; j < face.SubFaces.Count; ++j )
                        {
                            var otherSubFace = face.SubFaces[j];

                            if ( otherSubFace.Neighbor != subFace.Neighbor ) continue;
                            if ( otherSubFace.Material != subFace.Material ) continue;

                            if ( !subFace.FaceCuts.TryMerge( otherSubFace.FaceCuts ) )
                            {
                                continue;
                            }

                            face.SubFaces.RemoveAt( j );

                            merged = true;
                            mergeCount += 1;

                            break;
                        }
                    } while ( merged );
                }
            }

            return mergeCount;
        }

        public int GetSign( Vector3 pos )
        {
            if ( IsEmpty ) return -1;

            var sign = 1;

            foreach ( var face in _faces )
            {
                sign = Math.Min( sign, face.Plane.GetSign( pos ) );

                if ( sign == -1 ) break;
            }

            return sign;
        }

        public bool TryGetFace( CsgPlane plane, out Face face )
        {
            foreach ( var candidate in _faces )
            {
                if ( candidate.Plane.ApproxEquals( plane ) )
                {
                    face = candidate;
                    return true;
                }
            }

            face = default;
            return false;
        }

        private void AddFace( Face face )
        {
            _faces.Add( face );

            _vertexPropertiesInvalid = true;
            _neighborsInvalid = true;
        }

        private void RemoveFace( int index )
        {
            _faces.RemoveAt( index );

            _vertexPropertiesInvalid = true;
            _neighborsInvalid = true;
        }

        private void RemoveFace( Face face )
        {
            Assert.True( _faces.Remove( face ) );

            _vertexPropertiesInvalid = true;
            _neighborsInvalid = true;
        }

        private void UpdateFace( int index, Face face )
        {
            _faces[index] = face;

            _vertexPropertiesInvalid = true;
            _neighborsInvalid = true;
        }

        private void UpdateNeighbors()
        {
            if ( !_neighborsInvalid ) return;

            _neighborsInvalid = false;

            _neighbors.Clear();

            foreach ( var face in _faces )
            {
                foreach ( var subFace in face.SubFaces )
                {
                    if ( subFace.Neighbor == null )
                    {
                        continue;
                    }

                    Assert.True( subFace.Neighbor != this );

                    if ( _neighbors.TryGetValue( subFace.Neighbor, out var existingPlane ) )
                    {
                        CsgHelpers.AssertAreEqual( face.Plane, existingPlane );
                    }
                    else
                    {
                        _neighbors.Add( subFace.Neighbor, face.Plane );
                    }
                }
            }
        }

        private void UpdateVertices()
        {
            if ( !_vertexPropertiesInvalid ) return;

            _vertexPropertiesInvalid = false;
            _vertices.Clear();

            if ( IsEmpty )
            {
                _vertexAverage = Vector3.Zero;
                _vertexBBox = default;
                _volume = 0f;
                return;
            }

            var min = new Vector3( float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity );
            var max = new Vector3( float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity );
            var avgPos = Vector3.Zero;
            var posCount = 0;

            const float volumeScale = 1.638716e-5f;

            foreach ( var face in _faces )
            {
                var basis = face.Plane.GetHelper();

                foreach ( var cut in face.FaceCuts )
                {
                    if ( float.IsNegativeInfinity( cut.Min ) || float.IsPositiveInfinity( cut.Max ) )
                    {
                        _volume = float.PositiveInfinity;
                        _vertexBBox = new BBox( float.NegativeInfinity, float.PositiveInfinity );
                        _vertexAverage = 0f;
                        return;
                    }

                    var a = basis.GetPoint( cut, cut.Min );

                    min = Vector3.Min( min, a );
                    max = Vector3.Max( max, a );

                    avgPos += a;
                    posCount += 1;

                    var shouldAdd = true;

                    // This might be too expensive

                    foreach ( var vertex in _vertices )
                    {
                        if ( (vertex - a).IsNearlyZero( CsgHelpers.DistanceEpsilon * 0.5f ) )
                        {
                            shouldAdd = false;
                            break;
                        }
                    }

                    if ( shouldAdd )
                    {
                        _vertices.Add( a );
                    }
                }
            }

            _vertexAverage = posCount == 0 ? Vector3.Zero : avgPos / posCount;
            _vertexBBox = new BBox( min, max );

            var volume = 0f;

            foreach ( var face in _faces )
            {
                if ( face.FaceCuts.Count < 3 ) continue;

                var basis = face.Plane.GetHelper();

                var a = basis.GetPoint( face.FaceCuts[0], face.FaceCuts[0].Max ) - _vertexAverage;
                var b = basis.GetPoint( face.FaceCuts[1], face.FaceCuts[1].Max ) - _vertexAverage;

                for ( var i = 2; i < face.FaceCuts.Count; ++i )
                {
                    var c = basis.GetPoint( face.FaceCuts[i], face.FaceCuts[i].Max ) - _vertexAverage;

                    volume += Math.Abs( Vector3.Dot( a, Vector3.Cross( b, c ) ) );

                    b = c;
                }
            }

            _volume = volume * volumeScale / 6f;
        }

        ~CsgHull()
        {
            if ( Collider.IsValid() )
            {
                Log.Warning( "Collider not disposed!" );
            }
        }
    }
}
