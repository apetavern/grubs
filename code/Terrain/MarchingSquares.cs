using Grubs.Utils;
using Grubs.Utils.Extensions;
using Sandbox.UI.Tests;

namespace Grubs.Terrain;

public sealed class MarchingSquares
{
	private int Width { get; set; }
	private int Height { get; set; }

	// Face
	private readonly List<Vector3> _vertices = new();
	private readonly List<int> _triangles = new();

	// Extrusion
	private readonly Dictionary<int, List<Triangle>> _triangleDictionary = new();
	private readonly List<List<int>> _outlines = new();
	private readonly HashSet<int> _checkedVertices = new();

	private readonly ModelBuilder _builder = new();

	private const float LocalY = -32f;

	public Model GenerateModel( bool[,] grid )
	{
		Width = grid.GetLength( 0 );
		Height = grid.GetLength( 1 );

		var scale = GameConfig.TerrainScale;
		March( grid, scale );

		var vertexNormals = new List<Vector3>();
		for ( var i = 0; i < _triangles.Count; i += 3 )
		{
			var vertexA = _triangles[i];
			var vertexB = _triangles[i + 1];
			var vertexC = _triangles[i + 2];

			var edgeAb = _vertices[vertexB] - _vertices[vertexA];
			var edgeAc = _vertices[vertexC] - _vertices[vertexA];

			var areaWeightedNormal = Vector3.Cross( edgeAb, edgeAc );

			vertexNormals.Add( areaWeightedNormal );
			vertexNormals.Add( areaWeightedNormal );
			vertexNormals.Add( areaWeightedNormal );
		}

		var vertexTangents = new List<Vector3>();
		for ( var i = 0; i < _triangles.Count; i++ )
		{
			var t1 = Vector3.Cross( vertexNormals[i], Vector3.Forward );
			var t2 = Vector3.Cross( vertexNormals[i], Vector3.Up );

			vertexTangents.Add( t1.Length > t2.Length ? t1 : t2 );
		}

		// Convert Vector3 Vertices to Vert List
		var vertList = new List<Vert>();
		for ( var i = 0; i < _vertices.Count; i++ )
		{
			var texCoord = new Vector2( _vertices[i].x / 512, _vertices[i].z / 512 );
			vertList.Add( new Vert( _vertices[i], vertexNormals[i], vertexTangents[i], texCoord ) );
		}

		if ( !Host.IsClient )
			return _builder.Create();

		if ( !Enum.TryParse( GameConfig.TerrainType, out TerrainType matType ) )
		{
			Log.Error( $"Got invalid terrain type \"{GameConfig.TerrainType}\", reverting to {TerrainType.None}" );
			matType = TerrainType.None;
		}

		// TODO: Calculate normal/tangent.
		var mesh = new Mesh( Material.Load( matType.GetMaterial() ) )
		{
			Bounds = new BBox( new Vector3( 0, LocalY, 0 ), new Vector3( Width * scale, LocalY + 64, Height * scale ) )
		};

		mesh.CreateVertexBuffer( vertList.Count, Vert.Layout, vertList );
		mesh.CreateIndexBuffer( _triangles.Count, _triangles );

		_builder.AddMesh( mesh );

		return _builder.Create();
	}

	public Model CreateWallModel()
	{
		CalculateMeshOutlines();
		var wallVertices = new List<Vector3>();
		var wallTriangles = new List<int>();

		var scale = GameConfig.TerrainScale;
		const float wallHeight = 64f;

		if ( !Enum.TryParse( GameConfig.TerrainType, out TerrainType matType ) )
		{
			Log.Error( $"Got invalid terrain type \"{GameConfig.TerrainType}\", reverting to {TerrainType.None}" );
			matType = TerrainType.None;
		}

		foreach ( var outline in _outlines )
		{
			for ( var i = 0; i < outline.Count - 1; i++ )
			{
				var startIndex = wallVertices.Count;
				wallVertices.Add( _vertices[outline[i]] );
				wallVertices.Add( _vertices[outline[i + 1]] );
				wallVertices.Add( _vertices[outline[i]] - Vector3.Right * wallHeight );
				wallVertices.Add( _vertices[outline[i + 1]] - Vector3.Right * wallHeight );

				wallTriangles.Add( startIndex + 0 );
				wallTriangles.Add( startIndex + 2 );
				wallTriangles.Add( startIndex + 3 );

				wallTriangles.Add( startIndex + 3 );
				wallTriangles.Add( startIndex + 1 );
				wallTriangles.Add( startIndex + 0 );
			}
		}

		if ( Host.IsClient )
		{
			var vertList = new List<Vert>();
			foreach ( var vert in wallVertices )
			{
				vertList.Add( new Vert( vert, Vector3.Up, Vector3.Left, new Vector2( 0, 0 ) ) );
			}

			var wallMesh = new Mesh( Material.Load( matType.GetMaterial() ) )
			{
				Bounds = new BBox( 0, new Vector3( Width * scale, wallHeight, Height * scale ) )
			};

			wallMesh.CreateVertexBuffer( vertList.Count, Vert.Layout, vertList );
			wallMesh.CreateIndexBuffer( wallTriangles.Count, wallTriangles );

			_builder.AddMesh( wallMesh );
		}

		_builder.AddCollisionMesh( wallVertices.ToArray(), wallTriangles.ToArray() );
		return _builder.Create();
	}

	private void March( bool[,] terrainGrid, int scale )
	{
		for ( var x = 0; x < terrainGrid.GetLength( 0 ) - 1; x++ )
			for ( var z = 0; z < terrainGrid.GetLength( 1 ) - 1; z++ )
			{
				float xRes = x * scale;
				float zRes = z * scale;

				var middleTop = new Node( new Vector3( xRes + scale * 0.5f, LocalY, zRes ) );
				var middleRight = new Node( new Vector3( xRes + scale, LocalY, zRes + scale * 0.5f ) );
				var middleBottom = new Node( new Vector3( xRes + scale * 0.5f, LocalY, zRes + scale ) );
				var middleLeft = new Node( new Vector3( xRes, LocalY, zRes + scale * 0.5f ) );

				var topLeft = new Node( new Vector3( xRes, LocalY, zRes ) );
				var topRight = new Node( new Vector3( xRes + scale, LocalY, zRes ) );
				var bottomRight = new Node( new Vector3( xRes + scale, LocalY, zRes + scale ) );
				var bottomLeft = new Node( new Vector3( xRes, LocalY, zRes + scale ) );

				var c1 = terrainGrid[x, z];
				var c2 = terrainGrid[x + 1, z];
				var c3 = terrainGrid[x + 1, z + 1];
				var c4 = terrainGrid[x, z + 1];

				var marchCase = GetCase( c1, c2, c3, c4 );

				switch ( marchCase )
				{
					case 1:
						MeshFromPoints( middleLeft, middleBottom, bottomLeft );
						break;
					case 2:
						MeshFromPoints( bottomRight, middleBottom, middleRight );
						break;
					case 4:
						MeshFromPoints( topRight, middleRight, middleTop );
						break;
					case 8:
						MeshFromPoints( topLeft, middleTop, middleLeft );
						break;

					// 2 points:
					case 3:
						MeshFromPoints( middleRight, bottomRight, bottomLeft, middleLeft );
						break;
					case 6:
						MeshFromPoints( middleTop, topRight, bottomRight, middleBottom );
						break;
					case 9:
						MeshFromPoints( topLeft, middleTop, middleBottom, bottomLeft );
						break;
					case 12:
						MeshFromPoints( topLeft, topRight, middleRight, middleLeft );
						break;
					case 5:
						MeshFromPoints( middleTop, topRight, middleRight, middleBottom, bottomLeft, middleLeft );
						break;
					case 10:
						MeshFromPoints( topLeft, middleTop, middleRight, bottomRight, middleBottom, middleLeft );
						break;

					// 3 point:
					case 7:
						MeshFromPoints( middleTop, topRight, bottomRight, bottomLeft, middleLeft );
						break;
					case 11:
						MeshFromPoints( topLeft, middleTop, middleRight, bottomRight, bottomLeft );
						break;
					case 13:
						MeshFromPoints( topLeft, topRight, middleRight, middleBottom, bottomLeft );
						break;
					case 14:
						MeshFromPoints( topLeft, topRight, bottomRight, middleBottom, middleLeft );
						break;

					// 4 point:
					case 15:
						MeshFromPoints( topLeft, topRight, bottomRight, bottomLeft );
						// We still want to create walls for the map border, but skip over all other filled squares.
						if ( x == 0 || z == 0 || x == terrainGrid.GetLength( 0 ) - 2 || z == terrainGrid.GetLength( 1 ) - 2 )
							break;
						_checkedVertices.Add( topLeft.VertexIndex );
						_checkedVertices.Add( topRight.VertexIndex );
						_checkedVertices.Add( bottomRight.VertexIndex );
						_checkedVertices.Add( bottomLeft.VertexIndex );
						break;
				}
			}
	}

	private static int GetCase( bool a, bool b, bool c, bool d )
	{
		var num = 0;
		if ( a )
			num += 8;
		if ( b )
			num += 4;
		if ( c )
			num += 2;
		if ( d )
			num += 1;

		return num;
	}

	private void MeshFromPoints( params Node[] points )
	{
		AssignVertices( points );

		if ( points.Length >= 3 )
			CreateTriangle( points[0], points[1], points[2] );
		if ( points.Length >= 4 )
			CreateTriangle( points[0], points[2], points[3] );
		if ( points.Length >= 5 )
			CreateTriangle( points[0], points[3], points[4] );
		if ( points.Length >= 6 )
			CreateTriangle( points[0], points[4], points[5] );
	}

	private void AssignVertices( Node[] points )
	{
		foreach ( var point in points )
		{
			if ( point.VertexIndex != -1 )
				continue;

			point.VertexIndex = _vertices.Count;
			_vertices.Add( point.Position );
		}
	}

	private void CreateTriangle( Node a, Node b, Node c )
	{
		_triangles.Add( a.VertexIndex );
		_triangles.Add( b.VertexIndex );
		_triangles.Add( c.VertexIndex );

		var triangle = new Triangle( a.VertexIndex, b.VertexIndex, c.VertexIndex );
		AddTriangleToDictionary( triangle.VertexIndexA, triangle );
		AddTriangleToDictionary( triangle.VertexIndexB, triangle );
		AddTriangleToDictionary( triangle.VertexIndexC, triangle );
	}

	private void AddTriangleToDictionary( int vertexIndexKey, Triangle triangle )
	{
		if ( _triangleDictionary.ContainsKey( vertexIndexKey ) )
		{
			_triangleDictionary[vertexIndexKey].Add( triangle );
		}
		else
		{
			List<Triangle> triangleList = new() { triangle };
			_triangleDictionary.Add( vertexIndexKey, triangleList );
		}
	}

	private void CalculateMeshOutlines()
	{
		for ( var vertexIndex = 0; vertexIndex < _vertices.Count; vertexIndex++ )
		{
			if ( _checkedVertices.Contains( vertexIndex ) )
				continue;

			var newOutlineVertex = GetConnectedOutlineVertex( vertexIndex );
			if ( newOutlineVertex == -1 )
				continue;

			_checkedVertices.Add( vertexIndex );

			List<int> newOutline = new() { vertexIndex };
			_outlines.Add( newOutline );
			FollowOutline( newOutlineVertex, _outlines.Count - 1 );
			_outlines[^1].Add( vertexIndex );
		}
	}

	private void FollowOutline( int vertexIndex, int outlineIndex )
	{
		while ( true )
		{
			_outlines[outlineIndex].Add( vertexIndex );
			_checkedVertices.Add( vertexIndex );

			var nextVertexIndex = GetConnectedOutlineVertex( vertexIndex );
			if ( nextVertexIndex != -1 )
			{
				vertexIndex = nextVertexIndex;
				continue;
			}

			break;
		}
	}

	private int GetConnectedOutlineVertex( int vertexIndex )
	{
		var trianglesContainingVertex = _triangleDictionary[vertexIndex];
		foreach ( var triangle in trianglesContainingVertex )
		{
			for ( var j = 0; j < 3; j++ )
			{
				var vertexB = triangle[j];

				if ( vertexB == vertexIndex || _checkedVertices.Contains( vertexB ) )
					continue;

				if ( IsOutlineEdge( vertexIndex, vertexB ) )
					return vertexB;
			}
		}

		return -1;
	}

	private bool IsOutlineEdge( int vertexA, int vertexB )
	{
		var trianglesContainingVertexA = _triangleDictionary[vertexA];
		var sharedTriangleCount = 0;

		for ( var i = 0; i < trianglesContainingVertexA.Count; i++ )
		{
			if ( !trianglesContainingVertexA[i].Contains( vertexB ) )
				continue;

			sharedTriangleCount++;
			if ( sharedTriangleCount > 1 )
				break;
		}

		return sharedTriangleCount == 1;
	}

	/// <summary>
	/// Helper class representing a Node in-world and its vertex index.
	/// </summary>
	private class Node
	{
		public Vector3 Position { get; set; }
		public int VertexIndex { get; set; } = -1;

		public Node( Vector3 position )
		{
			Position = position;
		}
	}

	/// <summary>
	/// Helper class representing a triangle by its vertex indices.
	/// </summary>
	private readonly struct Triangle
	{
		public readonly int VertexIndexA;
		public readonly int VertexIndexB;
		public readonly int VertexIndexC;

		private readonly int[] _vertices;

		public Triangle( int a, int b, int c )
		{
			VertexIndexA = a;
			VertexIndexB = b;
			VertexIndexC = c;

			_vertices = new int[3];
			_vertices[0] = a;
			_vertices[1] = b;
			_vertices[2] = c;
		}

		public bool Contains( int vertexIndex )
		{
			return vertexIndex == VertexIndexA || vertexIndex == VertexIndexB || vertexIndex == VertexIndexC;
		}

		public int this[int i] => _vertices[i];
	}

	private struct Vert
	{
		public Vector3 Position;
		public Vector3 Normal;
		public Vector3 Tangent;
		public Vector2 TexCoord;

		public Vert( Vector3 position, Vector3 normal, Vector3 tangent, Vector2 texCoord )
		{
			Position = position;
			Normal = normal;
			Tangent = tangent;
			TexCoord = texCoord;
		}

		public static readonly VertexAttribute[] Layout =
		{
			new(VertexAttributeType.Position, VertexAttributeFormat.Float32),
			new(VertexAttributeType.Normal, VertexAttributeFormat.Float32),
			new(VertexAttributeType.Tangent, VertexAttributeFormat.Float32),
			new(VertexAttributeType.TexCoord, VertexAttributeFormat.Float32, 2)
		};
	}
}
