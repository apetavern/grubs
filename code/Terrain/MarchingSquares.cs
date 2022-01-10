using Sandbox;
using System.Collections.Generic;

namespace Grubs.Terrain
{
	public struct MarchingSquares
	{
		public static readonly Vector2 NW = new Vector2( -1f, 1f );
		public static readonly Vector2 N = new Vector2( 0, 1f );
		public static readonly Vector2 NE = new Vector2( 1f, 1f );
		public static readonly Vector2 E = new Vector2( 1f, 0 );
		public static readonly Vector2 SE = new Vector2( 1f, -1f );
		public static readonly Vector2 S = new Vector2( 0, -1f );
		public static readonly Vector2 SW = new Vector2( -1f, -1f );
		public static readonly Vector2 W = new Vector2( -1f, 0 );

		public static Vector2[] VertexOffsets = new Vector2[8] { NW, N, NE, E, SE, S, SW, W };

		public const float EdgeWidth = 64f;
		public const float TextureScale = 0.01f;
		public const bool Smoothing = true;

		// Removes duplicate vertices - experimental.
		public const bool MeshSimplification = true;

		public static Material Material = Material.Load( "materials/peterburroughs/dirt.vmat" );

		public static readonly int[,] CornersFromEdge = new int[8, 2]
		{
		{ -1, -1 },
		{ 0, 2 },
		{ -1, -1 },
		{ 2, 4 },
		{ -1, -1 },
		{ 4, 6 },
		{ -1, -1 },
		{ 6, 0 },
		};

		public static int[,] Triangles = new int[18, 12]{
		{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
		{ 7, 6, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
		{ 5, 4, 3, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
		{ 6, 4, 3, 7, 6, 3, -1, -1, -1, -1, -1, -1 },
		{ 3, 2, 1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
		{ 7, 6, 5, 5, 3, 1, 3, 2, 1, 7, 5, 1 }, // ambiguous
		{ 4, 2, 1, 5, 4, 1, -1, -1, -1, -1, -1, -1 },
		{ 4, 2, 1, 7, 4, 1, 7, 6, 4 , -1, -1, -1 },
		{ 7, 1, 0, -1, -1, -1, -1, -1, -1 , -1, -1, -1 },
		{ 6, 5, 0, 5, 1, 0, -1, -1, -1, -1, -1, -1 },
		{ 5, 4, 3, 5, 3, 1, 1, 0, 7, 1, 7, 5 }, // ambiguous
		{ 6, 1, 0, 6, 3, 1, 6, 4, 3, -1, -1, -1 },
		{ 7, 2, 0, 7, 3, 2, -1, -1, -1, -1, -1, -1 },
		{ 3, 2, 0, 5, 3, 0, 6, 5, 0, -1, -1, -1 },
		{ 7, 2, 0, 5, 4, 2, 7, 5, 2, -1, -1, -1 },
		{ 6, 2, 0, 6, 4, 2, -1, -1, -1, -1, -1, -1 },
		{ 7, 6, 5, 3, 2, 1, -1, -1, -1, -1, -1, -1 }, // ambiguous
		{ 5, 4, 3, 1, 0, 7, -1, -1, -1, -1, -1, -1 }, // ambiguous
	};

		public static int[,] EdgeOrder = new int[18, 4]{
		{ -1, -1, -1, -1 },
		{ 7, 5, -1, -1 },
		{ 5, 3, -1, -1 },
		{ 7, 3, -1, -1 },
		{ 3, 1, -1, -1 },
		{ 7, 1, 3, 5 },
		{ 5, 1, -1, -1 },
		{ 7, 1, -1, -1 },
		{ 1, 7, -1, -1 },
		{ 1, 5, -1, -1 },
		{ 1, 3, 5, 7 },
		{ 1, 3, -1, -1 },
		{ 3, 7, -1, -1 },
		{ 3, 5, -1, -1 },
		{ 5, 7, -1, -1 },
		{ -1, -1, -1, -1 },
		{ 3, 1, 7, 5 },
		{ 1, 7, 5, 3},
	};

		public static int[] TriangleCount = GetTriangleCount();
		#region TriangleCount calculations
		private static int[] GetTriangleCount()
		{
			int[] triCount = new int[18];
			for ( int i = 0; i < 18; i++ )
				triCount[i] = GetTriangles( i );

			return triCount;
		}

		private static int GetTriangles( int i )
		{
			for ( int j = 0; j < 12; j++ )
			{
				if ( Triangles[i, j] == -1 )
					return j;
			}
			return 12;
		}
		#endregion

		public static int[] EdgeCount = GetEdgeCount();
		#region EdgeCount calculations

		private static int[] GetEdgeCount()
		{
			int[] triCount = new int[18];
			for ( int i = 0; i < 18; i++ )
				triCount[i] = GetEdges( i );

			return triCount;
		}

		private static int GetEdges( int i )
		{
			for ( int j = 0; j < 4; j++ )
			{
				if ( EdgeOrder[i, j] == -1 )
					return j;
			}
			return 4;
		}
		#endregion

		public static int[] UniqueCount = new int[18];
		public static int[][] UniqueIndices = GetIndices();
		private static int[][] GetIndices()
		{
			int[][] uniqueIndices = new int[18][];

			for ( int i = 0; i < 18; i++ )
			{
				List<int> uniques = new List<int>();
				for ( int j = 0; j < TriangleCount[i]; j++ )
				{
					int index = Triangles[i, j];
					if ( !uniques.Contains( index ) )
						uniques.Add( index );
				}
				var uniqueArray = uniques.ToArray();
				uniqueIndices[i] = uniqueArray;
				UniqueCount[i] = uniqueArray.Length;
			}
			return uniqueIndices;
		}
	}
}
