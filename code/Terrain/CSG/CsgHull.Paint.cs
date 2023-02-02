using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox.Csg
{
    partial class CsgHull
    {
        private bool ShouldPaintSubFace( SubFace subFace, CsgMaterial material )
        {
            return subFace.Neighbor == null && (subFace.Material ?? Material) != (material ?? Material);
        }

        public bool Paint( CsgHull brush, CsgMaterial material )
        {
            var paintCuts = CsgHelpers.RentFaceCutList();
            var negCuts = CsgHelpers.RentFaceCutList();

            var changed = false;

            try
            {
                foreach ( var face in _faces )
                {
                    var anyToPaint = false;

                    foreach ( var subFace in face.SubFaces )
                    {
                        if ( ShouldPaintSubFace( subFace, material ) )
                        {
                            anyToPaint = true;
                            break;
                        }
                    }

                    if ( !anyToPaint )
                    {
                        continue;
                    }

                    changed = true;

					var helper = face.Plane.GetHelper();

                    paintCuts.Clear();
                    paintCuts.AddRange( face.FaceCuts );

                    foreach ( var brushFace in brush.Faces )
                    {
                        paintCuts.Split( helper.GetCut( brushFace.Plane ) );
                    }
                    
                    if ( paintCuts.IsDegenerate() ) continue;
                    if ( brush.GetSign( helper.GetAveragePos( paintCuts ) ) < 0 ) continue;

                    var avgPos = paintCuts.GetAveragePos();

                    if ( !face.FaceCuts.Contains( avgPos ) ) continue;

                    for ( var i = face.SubFaces.Count - 1; i >= 0; i-- )
                    {
                        var subFace = face.SubFaces[i];

                        if ( !ShouldPaintSubFace( subFace, material ) ) continue;

                        foreach ( var brushFace in brush.Faces )
                        {
                            var cut = helper.GetCut( brushFace.Plane );

                            if ( paintCuts.Count > 0 && !paintCuts.Contains( cut ) ) continue;

                            if ( !subFace.FaceCuts.Split( cut, negCuts ) )
                            {
                                continue;
                            }

                            face.SubFaces.Add( new SubFace
                            {
                                FaceCuts = new List<FaceCut>( negCuts ),
                                Material = subFace.Material
                            } );
                        }

                        if ( brush.GetSign( helper.GetAveragePos( subFace.FaceCuts ) ) < 0 )
                        {
                            continue;
                        }

                        subFace.Material = material;
                        face.SubFaces[i] = subFace;
                    }
                }

                if ( changed ) InvalidateMesh();

                return changed;
            }
            finally
            {
                CsgHelpers.Return( paintCuts );
                CsgHelpers.Return( negCuts );
            }
        }
    }
}
