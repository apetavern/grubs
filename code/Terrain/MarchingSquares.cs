using Grubs.Utils;

namespace Grubs.Terrain;

public class MarchingSquares
{
	// Face
	public List<Vector3> Vertices { get; set; } = new();
	public List<int> Triangles { get; set; } = new();

	// Extrusion
	public Dictionary<int, List<Triangle>> TriangleDictionary = new();
	public List<List<int>> Outlines = new();
	public HashSet<int> CheckedVertices = new();

	private readonly float localY = -32f;

	public ModelBuilder Builder = new();

	private int Width { get; set; }
	private int Height { get; set; }

	private Dictionary<TerrainTypes, string> TerrainMaterials = new()
	{
		{ TerrainTypes.NONE, "materials/environment/cereal.vmat" },
		{ TerrainTypes.DIRT, "materials/environment/dirt_rocks.vmat" },
		{ TerrainTypes.SAND, "materials/environment/sand_shells.vmat" },
		{ TerrainTypes.LAVA, "materials/environment/lava_rocks.vmat" },
	};

	public Model GenerateModel()
	{
		TerrainMap map = GrubsGame.Current.TerrainMap;
		Width = TerrainMap.Width;
		Height = TerrainMap.Height;

		var scale = GameConfig.TerrainScale;

		var TerrainGrid = map.TerrainGrid;
		March( TerrainGrid );

		var vertexNormals = new List<Vector3>();
		for ( int i = 0; i < Triangles.Count; i += 3 )
		{
			int vertexA = Triangles[i];
			int vertexB = Triangles[i + 1];
			int vertexC = Triangles[i + 2];

			var edgeAB = Vertices[vertexB] - Vertices[vertexA];
			var edgeAC = Vertices[vertexC] - Vertices[vertexA];

			var areaWeightedNormal = Vector3.Cross( edgeAB, edgeAC );

			vertexNormals.Add( areaWeightedNormal );
			vertexNormals.Add( areaWeightedNormal );
			vertexNormals.Add( areaWeightedNormal );
		}

		var vertexTangents = new List<Vector3>();
		for ( int i = 0; i < Triangles.Count; i++ )
		{
			var t1 = Vector3.Cross( vertexNormals[i], Vector3.Forward );
			var t2 = Vector3.Cross( vertexNormals[i], Vector3.Up );

			if ( t1.Length > t2.Length )
			{
				vertexTangents.Add( t1 );
			}
			else
			{
				vertexTangents.Add( t2 );
			}
		}

		// Convert Vector3 Vertices to Vert List
		var vertList = new List<Vert>();
		for ( int i = 0; i < Vertices.Count; i++ )
		{
			var texCoord = new Vector2( (float)(Vertices[i].x / 512), (float)(Vertices[i].z / 512) );
			vertList.Add( new Vert( Vertices[i], vertexNormals[i], vertexTangents[i], texCoord ) );
		}

		Enum.TryParse( GameConfig.TerrainType.ToUpper(), out TerrainTypes matType );
		// TODO: Calculate normal/tangent.
		var mesh = new Mesh( Material.Load( TerrainMaterials[matType] ) )
		{
			Bounds = new BBox( new Vector3( 0, localY, 0 ), new Vector3( Width * scale, localY + 64, Height * scale ) )
		};

		mesh.CreateVertexBuffer( vertList.Count, Vert.Layout, vertList );
		mesh.CreateIndexBuffer( Triangles.Count, Triangles );

		if ( Host.IsClient )
		{
			Builder.AddMesh( mesh );
		}

		// Builder.AddCollisionMesh( Vertices.ToArray(), Triangles.ToArray() );
		return Builder.Create();
	}

	public Model CreateWallModel()
	{
		CalculateMeshOutlines();
		List<Vector3> wallVertices = new();
		List<int> wallTriangles = new();

		var scale = GameConfig.TerrainScale;

		float wallHeight = 64f;

		Enum.TryParse( GameConfig.TerrainType.ToUpper(), out TerrainTypes matType );
		var wallMesh = new Mesh( Material.Load( TerrainMaterials[matType] ) )
		{
			Bounds = new BBox( 0, new Vector3( Width * scale, wallHeight, Height * scale ) )
		};

		foreach ( List<int> outline in Outlines )
		{
			for ( int i = 0; i < outline.Count - 1; i++ )
			{
				int startIndex = wallVertices.Count;
				wallVertices.Add( Vertices[outline[i]] );
				wallVertices.Add( Vertices[outline[i + 1]] );
				wallVertices.Add( Vertices[outline[i]] - Vector3.Right * wallHeight );
				wallVertices.Add( Vertices[outline[i + 1]] - Vector3.Right * wallHeight );

				wallTriangles.Add( startIndex + 0 );
				wallTriangles.Add( startIndex + 2 );
				wallTriangles.Add( startIndex + 3 );

				wallTriangles.Add( startIndex + 3 );
				wallTriangles.Add( startIndex + 1 );
				wallTriangles.Add( startIndex + 0 );
			}
		}

		var vertList = new List<Vert>();
		foreach ( var vert in wallVertices )
		{
			vertList.Add( new Vert( vert, Vector3.Up, Vector3.Left, new Vector2( 0, 0 ) ) );
		}

		wallMesh.CreateVertexBuffer( vertList.Count, Vert.Layout, vertList );
		wallMesh.CreateIndexBuffer( wallTriangles.Count, wallTriangles );

		if ( Host.IsClient )
		{
			Builder.AddMesh( wallMesh );
		}

		Builder.AddCollisionMesh( wallVertices.ToArray(), wallTriangles.ToArray() );

		return Builder.Create();
	}

	private void March( bool[,] TerrainGrid )
	{
		var scale = GameConfig.TerrainScale;

		for ( int x = 0; x < TerrainGrid.GetLength( 0 ) - 1; x++ )
			for ( int z = 0; z < TerrainGrid.GetLength( 1 ) - 1; z++ )
			{
				float xRes = x * scale;
				float zRes = z * scale;

				var middleTop = new Node( new Vector3( xRes + scale * 0.5f, localY, zRes ) );
				var middleRight = new Node( new Vector3( xRes + scale, localY, zRes + scale * 0.5f ) );
				var middleBottom = new Node( new Vector3( xRes + scale * 0.5f, localY, zRes + scale ) );
				var middleLeft = new Node( new Vector3( xRes, localY, zRes + scale * 0.5f ) );

				var topLeft = new Node( new Vector3( xRes, localY, zRes ) );
				var topRight = new Node( new Vector3( xRes + scale, localY, zRes ) );
				var bottomRight = new Node( new Vector3( xRes + scale, localY, zRes + scale ) );
				var bottomLeft = new Node( new Vector3( xRes, localY, zRes + scale ) );

				bool c1 = TerrainGrid[x, z];
				bool c2 = TerrainGrid[x + 1, z];
				bool c3 = TerrainGrid[x + 1, z + 1];
				bool c4 = TerrainGrid[x, z + 1];

				int marchCase = GetCase( c1, c2, c3, c4 );

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
						if ( x == 0 || z == 0 || x == TerrainGrid.GetLength( 0 ) - 2 || z == TerrainGrid.GetLength( 1 ) - 2 )
							break;
						CheckedVertices.Add( topLeft.VertexIndex );
						CheckedVertices.Add( topRight.VertexIndex );
						CheckedVertices.Add( bottomRight.VertexIndex );
						CheckedVertices.Add( bottomLeft.VertexIndex );
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
		for ( int i = 0; i < points.Length; i++ )
		{
			if ( points[i].VertexIndex == -1 )
			{
				points[i].VertexIndex = Vertices.Count;
				Vertices.Add( points[i].Position );
			}
		}
	}

	private void CreateTriangle( Node a, Node b, Node c )
	{
		Triangles.Add( a.VertexIndex );
		Triangles.Add( b.VertexIndex );
		Triangles.Add( c.VertexIndex );

		Triangle triangle = new Triangle( a.VertexIndex, b.VertexIndex, c.VertexIndex );
		AddTriangleToDictionary( triangle.VertexIndexA, triangle );
		AddTriangleToDictionary( triangle.VertexIndexB, triangle );
		AddTriangleToDictionary( triangle.VertexIndexC, triangle );
	}

	private void AddTriangleToDictionary( int vertexIndexKey, Triangle triangle )
	{
		if ( TriangleDictionary.ContainsKey( vertexIndexKey ) )
		{
			TriangleDictionary[vertexIndexKey].Add( triangle );
		}
		else
		{
			List<Triangle> triangleList = new();
			triangleList.Add( triangle );
			TriangleDictionary.Add( vertexIndexKey, triangleList );
		}
	}

	private void CalculateMeshOutlines()
	{
		for ( int vertexIndex = 0; vertexIndex < Vertices.Count; vertexIndex++ )
		{
			if ( !CheckedVertices.Contains( vertexIndex ) )
			{
				int newOutlineVertex = GetConnectedOutlineVertex( vertexIndex );
				if ( newOutlineVertex != -1 )
				{
					CheckedVertices.Add( vertexIndex );

					List<int> newOutline = new();
					newOutline.Add( vertexIndex );
					Outlines.Add( newOutline );
					FollowOutline( newOutlineVertex, Outlines.Count - 1 );
					Outlines[^1].Add( vertexIndex );
				}
			}
		}
	}

	private void FollowOutline( int vertexIndex, int outlineIndex )
	{
		Outlines[outlineIndex].Add( vertexIndex );
		CheckedVertices.Add( vertexIndex );

		int nextVertexIndex = GetConnectedOutlineVertex( vertexIndex );
		if ( nextVertexIndex != -1 )
		{
			FollowOutline( nextVertexIndex, outlineIndex );
		}
	}

	private int GetConnectedOutlineVertex( int vertexIndex )
	{
		List<Triangle> trianglesContainingVertex = TriangleDictionary[vertexIndex];
		for ( int i = 0; i < trianglesContainingVertex.Count; i++ )
		{
			Triangle triangle = trianglesContainingVertex[i];

			for ( int j = 0; j < 3; j++ )
			{
				int vertexB = triangle[j];

				if ( vertexB != vertexIndex && !CheckedVertices.Contains( vertexB ) )
				{
					if ( IsOutlineEdge( vertexIndex, vertexB ) )
					{
						return vertexB;
					}
				}
			}
		}

		return -1;
	}

	private bool IsOutlineEdge( int vertexA, int vertexB )
	{
		List<Triangle> trianglesContainingVertexA = TriangleDictionary[vertexA];
		int sharedTriangleCount = 0;

		for ( int i = 0; i < trianglesContainingVertexA.Count; i++ )
		{
			if ( trianglesContainingVertexA[i].Contains( vertexB ) )
			{
				sharedTriangleCount++;
				if ( sharedTriangleCount > 1 )
				{
					break;
				}
			}
		}

		return sharedTriangleCount == 1;
	}

	/// <summary>
	/// Helper class representing a Node in-world and its vertex index.
	/// </summary>
	public class Node
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
	public struct Triangle
	{
		public int VertexIndexA;
		public int VertexIndexB;
		public int VertexIndexC;

		public int[] Vertices;

		public Triangle( int a, int b, int c )
		{
			VertexIndexA = a;
			VertexIndexB = b;
			VertexIndexC = c;

			Vertices = new int[3];
			Vertices[0] = a;
			Vertices[1] = b;
			Vertices[2] = c;
		}

		public bool Contains( int vertexIndex )
		{
			return vertexIndex == VertexIndexA || vertexIndex == VertexIndexB || vertexIndex == VertexIndexC;
		}

		public int this[int i]
		{
			get
			{
				return Vertices[i];
			}
		}
	}

	public struct Vert
	{
		public Vector3 position;
		public Vector3 normal;
		public Vector3 tangent;
		public Vector2 texCoord;

		public Vert( Vector3 position, Vector3 normal, Vector3 tangent, Vector2 texCoord )
		{
			this.position = position;
			this.normal = normal;
			this.tangent = tangent;
			this.texCoord = texCoord;
		}

		public static readonly VertexAttribute[] Layout = new VertexAttribute[4]
		{
			new VertexAttribute(VertexAttributeType.Position, VertexAttributeFormat.Float32),
			new VertexAttribute(VertexAttributeType.Normal, VertexAttributeFormat.Float32),
			new VertexAttribute(VertexAttributeType.Tangent, VertexAttributeFormat.Float32),
			new VertexAttribute(VertexAttributeType.TexCoord, VertexAttributeFormat.Float32, 2)
		};
	}
}
