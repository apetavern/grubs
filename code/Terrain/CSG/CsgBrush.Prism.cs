using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox.Csg
{
    partial class CsgBrush
    {
        public static CsgBrush CreatePrism( IList<Vector3> baseVertices, Vector3 extrude )
        {
            Assert.True( baseVertices.Count >= 3 );

            var baseNormal = Vector3.Cross( baseVertices[1] - baseVertices[0], baseVertices[2] - baseVertices[1] ).Normal;

            if ( Vector3.Dot( extrude, baseNormal ) < 0f )
            {
                baseNormal = -baseNormal;
            }

            var basePlane = new CsgPlane( baseNormal, baseVertices[0] );
            var extrudePlane = new CsgPlane( -baseNormal, baseVertices[0] + extrude );

            var basePlaneHelper = basePlane.GetHelper();

            var polygonVertices = new List<Vector2>();

            foreach ( var baseVertex in baseVertices )
            {
                polygonVertices.Add( basePlaneHelper.Project( baseVertex ) );
            }

            var polygonVertCounts = new List<int>();

            CsgHelpers.MakeConvex( polygonVertices, polygonVertCounts );

            var brush = new CsgBrush
            {
                ConvexSolids = new List<ConvexSolid>()
            };

            var offset = 0;

            foreach ( var count in polygonVertCounts )
            {
                var solid = new CsgBrush.ConvexSolid
                {
                    Planes = new List<Plane>( count + 2 )
                };

                brush.ConvexSolids.Add( solid );

                solid.Planes.Add( basePlane );
                solid.Planes.Add( extrudePlane );

                var prev = basePlaneHelper.GetPoint( polygonVertices[offset + count - 1] );

                for ( var i = 0; i < count; i++ )
                {
                    var next = basePlaneHelper.GetPoint( polygonVertices[offset + i] );

                    var faceNormal = Vector3.Cross( extrude, next - prev ).Normal;

                    solid.Planes.Add( new Plane { Normal = faceNormal, Distance = Vector3.Dot( faceNormal, next ) } );

                    prev = next;
                }

                offset += count;
            }

            return brush;
        }
    }
}
