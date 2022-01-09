using Sandbox;
using System;
using System.Collections.Generic;

namespace Grubs.Terrain
{
	public class TreeNode
	{
		public int Level { get; private set; }
		public Vector2 Center { get; private set; }
		public int Extents => 1 << Quadtree.ExtentShifts >> Level;
		public TreeNode Parent { get; private set; }
		public TreeNode[] Children { get; private set; }
		public float[] CornerValues { get; private set; } = new float[4] { 0, 0, 0, 0 };
		public float CenterValue { get; private set; } = 0;
		public SolidState State { get; private set; }
		public bool IsLeaf { get; private set; } = true;
		public bool IsRoot { get; private set; }
		public bool Dirty { get; set; } = false;
		public bool Cleaned { get; set; } = false;
		public DataRange Range { get; private set; }
		public enum SolidState
		{
			Empty,
			Partial,
			Full
		}

		public TreeNode( int childIndex = -1, TreeNode parent = null, int level = 0, SolidState? state = null )
		{
			Parent = parent;
			Level = level;

			IsRoot = childIndex == -1;
			Center = IsRoot ? Vector2.Zero : Parent.Center + Quadtree.CellOffsets[childIndex] * Extents;

			if ( IsRoot )
				Range = new DataRange( 0, Quadtree.MaxResolution, 0, Quadtree.MaxResolution );
			else
			{
				// newly split nodes have no idea what their center value should be.
				// should probably set an approximation through their parents split method
				Range = Parent.Range.ClampedToQuadrant( childIndex );
			}

			if ( Level == Quadtree.Levels - 1 )
				GetValuesFromGrid();

			if ( state != null && state != SolidState.Partial )
			{
				State = (SolidState)state;

				float value = State == SolidState.Full ? -Extents * sqrt2 : Extents * sqrt2;

				CenterValue = value;
			}

			if ( Level < Quadtree.ModelLevel )
			{
				State = SolidState.Partial;
				Split();
			}
		}

		void GetValuesFromGrid()
		{
			CornerValues[0] = Quadtree.GridValues[Range.MinX, Range.MaxY];
			CornerValues[1] = Quadtree.GridValues[Range.MaxX, Range.MaxY];
			CornerValues[2] = Quadtree.GridValues[Range.MaxX, Range.MinY];
			CornerValues[3] = Quadtree.GridValues[Range.MinX, Range.MinY];
		}

		public void UpdateGrid( SDF sdf )
		{
			float stepSize = 4 << Quadtree.ExtentShifts >> Quadtree.Levels;
			Vector2 step = Vector2.Zero;
			Vector2 start = -Extents;
			for ( int x = Range.MinX; x < Range.MaxX; x++ )
			{
				step.x = stepSize * x;
				for ( int y = Range.MinY; y < Range.MaxY; y++ )
				{
					step.y = stepSize * y;
					Vector2 pos = start + step;

					float value = MathSDF.Operate( Quadtree.GridValues[x, y], sdf.GetDistance( pos ), sdf.Type );

					Quadtree.GridValues[x, y] = value;
				}
			}
		}

		public void Update( SDF sdf )
		{
			if ( IsRoot )
				UpdateGrid( sdf );

			float centerSDF = sdf.GetDistance( Center );

			CenterValue = MathSDF.Operate( CenterValue, centerSDF, sdf.Type );

			// Multiply by 0.5 to simulate doubled extents, this is done to make sure that
			// corner values shared between different nodes stay in sync.
			bool possibleEffect = EstimatedState( centerSDF * 0.5f ) == SolidState.Partial;

			bool newValues = false;

			if ( Level == Quadtree.Levels - 1 && possibleEffect )
			{
				for ( int i = 0; i < 4; i++ )
				{
					float cornerSDF = sdf.GetDistance( Center + Quadtree.CellOffsets[i] * Extents );
					float newValue = MathSDF.Operate( CornerValues[i], cornerSDF, sdf.Type );

					if ( !newValues && newValue != CornerValues[i] )
					{
						newValues = true;
						break;
					}
				}
				GetValuesFromGrid();
			}

			bool shouldSplit = ShouldSplit( out SolidState previousState );
			if ( State != previousState )
				possibleEffect = true;

			if ( possibleEffect )
				Cleaned = false;

			if ( IsLeaf == shouldSplit || newValues || State != previousState )
				DirtyAncestors();

			if ( shouldSplit && IsLeaf )
				Split( previousState );
			else if ( !shouldSplit && Level > Quadtree.ModelLevel - 1 )
				Unsplit();

			// update children
			if ( !IsLeaf && possibleEffect )
				foreach ( TreeNode node in Children )
					node.Update( sdf );

			if ( IsRoot )
				Clean();
		}

		public void Clean( bool goingDown = true )
		{
			if ( goingDown )
			{
				if ( Cleaned )
					return;

				if ( !IsLeaf && Level < Quadtree.Levels - 1 )
				{
					foreach ( TreeNode node in Children )
						node.Clean();
				}
				else // hit a bottom leaf
				{
					Parent.Clean( false );
				}
			}
			else if ( !IsLeaf && Level > Quadtree.ModelLevel - 1 )  // going up
			{
				if ( ShouldClean( out bool full ) )
				{
					float extents = Extents * sqrt2;
					float value = CenterValue;

					CenterValue = full ? MathF.Min( value, -extents ) : MathF.Max( value, extents );
					State = full ? SolidState.Full : SolidState.Empty;

					Unsplit();

					if ( Level != Quadtree.ModelLevel && Parent != null )
						Parent.Clean( false );
				}
				Cleaned = true;
			}
		}

		public bool ShouldClean( out bool full )
		{
			full = SolidState.Full == State;

			SolidState state = Children[0].State;

			if ( state == SolidState.Partial )
				return false;

			full = state == SolidState.Full;

			for ( int i = 1; i < 4; i++ )
			{
				TreeNode child = Children[i];
				if ( child.State != state )
					return false;
			}

			return true;
		}

		public List<TreeNode> CellsAtLevel( int level )
		{
			List<TreeNode> cells = new List<TreeNode>();

			if ( Level > level )
				return cells;

			if ( Level <= level )
			{
				if ( IsLeaf || Level == level )
				{
					cells.Add( this );
				}
				else
				{
					foreach ( TreeNode cell in Children )
					{
						cells.AddRange( cell.CellsAtLevel( level ) );
					}
				}
			}
			return cells;
		}

		public void DirtyAncestors()
		{
			if ( Level >= Quadtree.ModelLevel )
			{
				Dirty = true;
				if ( Level > Quadtree.ModelLevel && Parent != null )
					Parent.DirtyAncestors();
			}
		}

		private static float sqrt2 = MathF.Sqrt( 2f );

		static TreeNode[] noChildren = new TreeNode[0];

		public void GetMeshData( out MeshData surfaceData, out MeshData edgeData )
		{
			surfaceData = new MeshData();
			edgeData = new MeshData();

			if ( Level == Quadtree.ModelLevel )
			{
				GetMeshData( surfaceData, edgeData );
			}
		}

		public void GetMeshData( MeshData surfaceData, MeshData edgeData )
		{
			if ( State == SolidState.Empty ) // no mesh data to get
				return;

			if ( !IsLeaf ) // recurse on children
			{
				foreach ( TreeNode node in Children )
					node.GetMeshData( surfaceData, edgeData );
			}
			else // add to data unless node is empty
			{
				int marchCase = State == SolidState.Full ? 15 : 0;
				if ( State == SolidState.Partial )
				{
					for ( int i = 0; i < 4; i++ )
						marchCase = (marchCase << 1) + (CornerValues[i] < 0f ? 1 : 0);
				}

				/*
				// fix for ambiguous cases (this is kinda broken still lol)
				if ( marchCase == 5 || marchCase == 10 )
				{
					int score = 0;

					for ( int i = 0; i < 4; i++ )
					{
						int next = (i + 1) % 4;
						float aNoise = CornerValues[i];
						float bNoise = CornerValues[next];
						float t = (0 - aNoise) / (bNoise - aNoise);
						if ( (i % 2 == 1 && marchCase == 5) || (i % 2 == 0 && marchCase == 10) )
							t = 1f - t;
						score += t > 0.5f ? 1 : -1;
					}

					if ( score < 0 )
						marchCase = marchCase == 5 ? 16 : 17;
				}
				*/

				Vector3 edgeOffset = Vector3.Up * MarchingSquares.EdgeWidth * 0.5f;

				// generate all vertices used in the case
				int uniqueCount = MarchingSquares.UniqueCount[marchCase];
				TerrainVertex[] surfaceVertices = new TerrainVertex[uniqueCount];
				int[] vertexOrder = new int[8];
				for ( int i = 0; i < uniqueCount; i++ )
				{
					int index = MarchingSquares.UniqueIndices[marchCase][i];

					bool isEdge = index % 2 == 1;

					Vector2 pos;
					if ( MarchingSquares.Smoothing && isEdge ) // apply smoothing
					{
						int a = MarchingSquares.CornersFromEdge[index, 0];
						int b = MarchingSquares.CornersFromEdge[index, 1];

						float aNoise = CornerValues[a >> 1];
						float bNoise = CornerValues[b >> 1];

						Vector2 aPos = MarchingSquares.VertexOffsets[a];
						Vector2 bPos = MarchingSquares.VertexOffsets[b];

						float t = (0 - aNoise) / (bNoise - aNoise);
						Vector2 offset = aPos + t * (bPos - aPos);

						pos = Center + offset * Extents;
					}
					else
						pos = Center + MarchingSquares.VertexOffsets[index] * Extents;

					Vector3 vertexPos = (Vector3)pos + edgeOffset;
					Vector2 uv = MarchingSquares.TextureScale * (Vector2)vertexPos;
					surfaceVertices[i] = new TerrainVertex( vertexPos, Vector3.Up, Vector3.Right, uv );
					//new EdgeVertex( (Vector3)pos + edgeOffset, Vector3.Up, vertColor );
					vertexOrder[index] = i;
				}

				int triCount = MarchingSquares.TriangleCount[marchCase];
				int[] surfaceIndices = new int[triCount];
				for ( int i = 0; i < triCount; i++ )
					surfaceIndices[i] = surfaceData.VertexCount + vertexOrder[MarchingSquares.Triangles[marchCase, i]];

				int edgeCount = MarchingSquares.EdgeCount[marchCase];
				TerrainVertex[] edgeVertices = new TerrainVertex[edgeCount << 1];
				int[] edgeIndices = new int[edgeCount * 3];

				for ( int i = 0; i < edgeCount; i += 2 )
				{
					int firstIndex = MarchingSquares.EdgeOrder[marchCase, i];
					int secondIndex = MarchingSquares.EdgeOrder[marchCase, i + 1];

					Vector3 a = surfaceVertices[vertexOrder[firstIndex]].position;
					Vector3 b = surfaceVertices[vertexOrder[secondIndex]].position;

					Vector3 p0 = a;
					Vector3 p1 = b;
					Vector3 p2 = b - edgeOffset * 2;
					Vector3 p3 = a - edgeOffset * 2;

					Vector3 d0 = (p0 - p1).Normal;
					Vector3 d1 = (p0 - p2).Normal;
					Vector3 normal = Vector3.Cross( d0, d1 );

					Vector2 uv0 = MarchingSquares.TextureScale * (Vector2)p0;
					Vector2 uv1 = MarchingSquares.TextureScale * (Vector2)p1;
					Vector2 uv2 = uv0 - uv0.Normal * MarchingSquares.TextureScale * MarchingSquares.EdgeWidth;
					Vector2 uv3 = uv1 - uv1.Normal * MarchingSquares.TextureScale * MarchingSquares.EdgeWidth;



					int vert = i * 2;
					edgeVertices[vert] = new TerrainVertex( p0, normal, Vector3.Up, uv0 );
					edgeVertices[vert + 1] = new TerrainVertex( p1, normal, Vector3.Up, uv1 );
					edgeVertices[vert + 2] = new TerrainVertex( p2, normal, Vector3.Up, uv2 );
					edgeVertices[vert + 3] = new TerrainVertex( p3, normal, Vector3.Up, uv3 );

					int tri = i * 3;
					int vertOffset = vert + edgeData.VertexCount;
					edgeIndices[tri] = vertOffset;
					edgeIndices[tri + 1] = vertOffset + 1;
					edgeIndices[tri + 2] = vertOffset + 2;
					edgeIndices[tri + 3] = vertOffset + 2;
					edgeIndices[tri + 4] = vertOffset + 3;
					edgeIndices[tri + 5] = vertOffset;
				}

				surfaceData.AddData( surfaceVertices, surfaceIndices );
				edgeData.AddData( edgeVertices, edgeIndices );
			}
		}

		Color32 vertColor = new Color32( 255, 87, 51 );

		private SolidState EstimatedState( float? value = null )
		{
			float estimationValue = value ?? CenterValue;
			float extents = Extents * sqrt2;
			bool possiblyEmpty = estimationValue > -extents;
			bool possiblyFull = estimationValue < extents;
			bool possiblyPartial = possiblyEmpty && possiblyFull;

			return possiblyPartial ? SolidState.Partial : possiblyFull ? SolidState.Full : SolidState.Empty;
		}

		private bool ShouldSplit( out SolidState previousState )
		{
			previousState = State;
			State = EstimatedState();

			if ( Level == Quadtree.Levels - 1 && State == SolidState.Partial )
			{
				int marchCase = CornerValues[0] < 0f ? 1 : 0;
				for ( int i = 1; i < 4; i++ )
					marchCase = (marchCase << 1) + (CornerValues[i] < 0f ? 1 : 0);

				switch ( marchCase )
				{
					case 0:
						State = SolidState.Empty;
						break;
					case 15:
						State = SolidState.Full;
						break;
					default:
						State = SolidState.Partial;
						break;
				}
			}

			if ( Level == Quadtree.Levels - 1 )
				return false;

			if ( Level < Quadtree.ModelLevel )
				return true;

			if ( State == SolidState.Partial )
				return true;

			return false;
		}

		public void Unsplit()
		{
			IsLeaf = true;
			Children = noChildren;
		}

		public void Split( SolidState? toState = null )
		{
			IsLeaf = false;

			int nextLevel = Level + 1;
			Children = new TreeNode[4];
			for ( int i = 0; i < 4; i++ )
				Children[i] = new TreeNode( i, this, nextLevel, toState );
		}

		private static Color[] stateColors = new Color[3]
		{
		Color.Red.WithAlpha(0.25f),
		Color.Yellow,
		Color.Green.WithAlpha(0.5f)
		};
		public void DrawLeaves()
		{
			if ( IsLeaf )
				Draw();
			else
				foreach ( TreeNode cell in Children )
					cell.DrawLeaves();
		}

		public void Draw()
		{
			Color stateColor = stateColors[(int)State];

			Vector2 a = Extents - 0.5f;
			Vector2 b = new Vector2( -a.x, a.y );

			DebugOverlay.Line( Center + a, Center + b, stateColor );
			DebugOverlay.Line( Center + b, Center - a, stateColor );
			DebugOverlay.Line( Center - a, Center - b, stateColor );
			DebugOverlay.Line( Center - b, Center + a, stateColor );

			float[] cornerValues = new float[4];
			cornerValues[0] = Quadtree.GridValues[Range.MinX, Range.MaxY];
			cornerValues[1] = Quadtree.GridValues[Range.MaxX, Range.MaxY];
			cornerValues[2] = Quadtree.GridValues[Range.MaxX, Range.MinY];
			cornerValues[3] = Quadtree.GridValues[Range.MinX, Range.MinY];
		}

		public void Draw( Color color, float time = 0f )
		{
			Vector3 a = new Vector3( Extents - 0.5f, Extents - 0.5f, 0 );
			Vector3 b = new Vector3( -a.x, a.y, 0 );

			Vector3 pos = new Vector3( Center.x, Center.y, MarchingSquares.EdgeWidth * 0.5f );

			DebugOverlay.Line( pos + a, pos + b, color, time );
			DebugOverlay.Line( pos + b, pos - a, color, time );
			DebugOverlay.Line( pos - a, pos - b, color, time );
			DebugOverlay.Line( pos - b, pos + a, color, time );
		}
	}
}
