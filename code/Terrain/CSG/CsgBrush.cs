using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox.Csg
{
    [GameResource("CSG Brush", "csg", "A simple mesh that can be used to modify a CsgSolid.", Icon = "brush")]
    public partial class CsgBrush : GameResource
    {
        public static CsgMaterial DefaultMaterial { get; set; }

        public struct ConvexSolid
        {
            public List<Plane> Planes { get; set; }
        }

        public struct Plane
        {
            public static implicit operator CsgPlane( Plane plane ) =>
                new CsgPlane( plane.Normal.Normal, plane.Distance );

            public static implicit operator Plane( CsgPlane plane ) =>
                new Plane { Normal = plane.Normal, Distance = plane.Distance };

            public Vector3 Normal { get; set; }
            public float Distance { get; set; }
        }

        public List<ConvexSolid> ConvexSolids { get; set; }

        [HideInEditor]
        public Model Model
        {
            get
            {
                UpdateModel();

                return _model;
            }
        }

        private List<CsgHull> _hulls;
        private Model _model;

        public int CreateHulls( List<CsgHull> outHulls )
        {
            UpdateSolids();

            foreach ( var hull in _hulls )
            {
                outHulls.Add( hull.Clone() );
            }

            return _hulls.Count;
        }

        private void UpdateSolids()
        {
            if ( _hulls != null ) return;

            _hulls = new List<CsgHull>();

            if ( ConvexSolids == null ) return;

            foreach ( var solidInfo in ConvexSolids )
            {
                var hull = new CsgHull { Material = DefaultMaterial };

                if ( solidInfo.Planes != null )
                {
                    foreach ( var plane in solidInfo.Planes )
                    {
                        hull.Clip( plane );
                    }
                }

                if ( hull.IsEmpty )
                {
                    continue;
                }

                if ( !hull.IsFinite )
                {
                    Log.Warning( "Incomplete convex solid" );
                    continue;
                }

                _hulls.Add( hull );
            }
        }

        private void UpdateModel()
        {
            if ( _model != null ) return;

            UpdateSolids();

            var meshes = new Dictionary<int, Mesh>();

            Assert.True( CsgSolid.UpdateMeshes( meshes, _hulls ) );

            var modelBuilder = new ModelBuilder();

            foreach ( var (_, mesh) in meshes )
            {
                modelBuilder.AddMesh( mesh );
            }

            _model = modelBuilder.Create();
        }

        protected override void PostLoad()
        {
            base.PostLoad();

            _hulls = null;
            _model = null;
        }

        protected override void PostReload()
        {
            base.PostReload();

            _hulls = null;
            _model = null;
        }

        public static CsgBrush Deserialize( ref NetRead reader )
        {
            var resourceId = reader.Read<int>();

            if ( resourceId != 0 )
            {
                var brush = ResourceLibrary.Get<CsgBrush>( resourceId );

                Assert.NotNull( brush );

                return brush;
            }

            var solidCount = reader.Read<int>();
            var solids = new List<ConvexSolid>( solidCount );

            for ( var i = 0; i < solidCount; i++ )
            {
                var planeCount = reader.Read<int>();

                var solid = new ConvexSolid
                {
                    Planes = new List<Plane>( planeCount )
                };

                solids.Add( solid );

                for ( var j = 0; j < planeCount; j++ )
                {
                    solid.Planes.Add( reader.Read<Plane>() );
                }
            }

            return new CsgBrush
            {
                ConvexSolids = solids
            };
        }

        public void Serialize( NetWrite writer )
        {
            writer.Write( ResourceId );

            if ( ResourceId != 0 )
            {
                return;
            }

            writer.Write( ConvexSolids.Count );

            foreach ( var solid in ConvexSolids )
            {
                writer.Write( solid.Planes.Count );

                foreach ( var plane in solid.Planes )
                {
                    writer.Write( plane );
                }
            }
        }
    }
}
