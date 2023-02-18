using Sandbox.Csg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grubs.Terrain.CSG;
[Prefab, Category( "CSG" )]
public partial class CSGBrushPrefab : ModelEntity
{
	[Prefab]
	public bool PrismModel { get; set; }

	public CsgBrush GeneratedBrush;
	public static CSGBrushPrefab FromPrefab( string prefabName )
	{
		if ( PrefabLibrary.TrySpawn<CSGBrushPrefab>( prefabName, out var brush ) )
		{
			//brush.GenerateBrush();
			//var generatedBrush = brush.GeneratedBrush;
			return brush;
		}

		return null;
	}

	IList<T> ToIList<T>( List<T> t )
	{
		return t;
	}

	public void GenerateBrush()
	{
		if ( !PrismModel )
		{
			GeneratedBrush = new CsgBrush();
			List<CsgBrush.Plane> GeneratedPlanes = new List<CsgBrush.Plane>();
			foreach ( var item in Children )
			{
				CsgBrush.Plane plane = new CsgBrush.Plane();
				plane.Distance = -item.LocalPosition.Length;
				plane.Normal = -item.Rotation.Up;
				GeneratedPlanes.Add( plane );
			}
			CsgBrush.ConvexSolid solid = new CsgBrush.ConvexSolid();
			solid.Planes = GeneratedPlanes;
			List<CsgBrush.ConvexSolid> solids = new List<CsgBrush.ConvexSolid>();
			solids.Add( solid );
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
