using Sandbox;
using System.Linq;
using Sandbox.UI;
using System;
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

		// removes dupe vertices, might crash tho idk
		public const bool MeshSimplification = true;

		public static Material Material = Material.Load( "materials/peterburroughs/dirt.vmat" );// Material.Load( "materials/default/vertex_color.vmat" );

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

		/*
		[Event.Tick]
		public static void Tick()
		{
			DebugOverlay.Line( Vector2.Zero, E * 100, Color.Red );
			DebugOverlay.Line( Vector2.Zero, N * 100, Color.Green );

			for ( int m = 0; m < 18; m++ )
			{
				float xOffset = (m % 4) * 50 - 75;
				float yOffset = (m / 4) * 50 - 75;
				Vector2 offset = new Vector2( xOffset, yOffset );

				DebugOverlay.Line( offset + NW * 16, offset + NE * 16, Color.Black );
				DebugOverlay.Line( offset + NE * 16, offset + SE * 16, Color.Black );
				DebugOverlay.Line( offset + SE * 16, offset + SW * 16, Color.Black );
				DebugOverlay.Line( offset + SW * 16, offset + NW * 16, Color.Black );

				for ( int i = 0; i < 8; i++ )
				{
					DebugOverlay.Text( offset + VertexOffsets[i] * 16, i.ToString(), Color.Red );
				}

				int triCount = TriangleCount[m];

				for ( int i = 0; i < triCount; i += 3 )
				{
					Vector2[] verts = new Vector2[3];

					Vector2 pos;
					for ( int j = 0; j < 3; j++ )
					{
						int index = Triangles[m, i + j];
						bool edgeVertex = index % 2 == 1;

						if ( edgeVertex )
						{
							int a = CornersFromEdge[index, 0];
							int b = CornersFromEdge[index, 1];

							float t = 0.5f + MathF.Sin( Time.Now * 2f ) * 0.35f;

							Vector2 aPos = VertexOffsets[a];
							Vector2 bPos = VertexOffsets[b];

							Vector2 vertexOffset = aPos * t + (1f - t) * bPos;

							pos = offset + vertexOffset * 15;
						}
						else pos = offset + VertexOffsets[Triangles[m, i + j]] * 15;

						verts[j] = pos;
					}

					Vector2 v0 = verts[0];
					Vector2 v1 = verts[1];
					Vector2 v2 = verts[2];

					DebugOverlay.Line( v0, v1, Color.White );
					DebugOverlay.Line( v1, v2, Color.White );
					DebugOverlay.Line( v2, v0, Color.White );
				}
			}
		}
		*/
	}
}
