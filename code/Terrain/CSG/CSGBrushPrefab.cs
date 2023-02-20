using Sandbox.Csg;

namespace Grubs.Terrain.CSG;

[Prefab, Category( "CSG" )]
public partial class CsgBrushPrefab : ModelEntity
{
	[Prefab]
	public bool PrismModel { get; set; }

	public CsgBrush GeneratedBrush;
	public static CsgBrushPrefab FromPrefab( string prefabName )
	{
		if ( PrefabLibrary.TrySpawn<CsgBrushPrefab>( prefabName, out var brush ) )
		{
			return brush;
		}

		return null;
	}

	public void GenerateBrush()
	{
		if ( !PrismModel )
		{
			GeneratedBrush = new CsgBrush();
			var generatedPlanes = new List<CsgBrush.Plane>();
			foreach ( var item in Children )
			{
				var plane = new CsgBrush.Plane();
				plane.Distance = -item.LocalPosition.Length;
				plane.Normal = -item.Rotation.Up;
				generatedPlanes.Add( plane );
			}

			var solid = new CsgBrush.ConvexSolid
			{
				Planes = generatedPlanes
			};

			var solids = new List<CsgBrush.ConvexSolid>();
			GeneratedBrush.ConvexSolids = solids;

		}
		else
		{
			IList<Vector3> verts = new List<Vector3>();
			foreach ( var item in Model.GetVertices() )
			{
				verts.Add( item.Position );
			}

			GeneratedBrush = CsgBrush.CreatePrism( verts, Children[0].LocalPosition );
		}
	}
}
