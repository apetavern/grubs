using Sandbox;
using System;
using System.Collections.Generic;

namespace Grubs.Terrain
{
	public static partial class Quadtree
	{
		public static Vector2[] CellOffsets = new Vector2[4] {
			new Vector2( -1f, 1f ),
			new Vector2( 1f, 1f ),
			new Vector2( 1f, -1f ),
			new Vector2( -1f, -1f )
		};

		public const int Levels = 8; // Algorithm runs from 0 to n-1.
		public const int MaxResolution = 1 << (Levels - 1);
		public const int ExtentShifts = 10; // ExtentShifts should be a minimum of Levels + 1.
		public const int Extents = 1 << ExtentShifts;

		public static float[,] GridValues = CreateGrid();

		public static TreeNode RootCell = null;

		public static float[,] CreateGrid()
		{
			GridValues = new float[MaxResolution + 1, MaxResolution + 1];

			float stepSize = 4 << ExtentShifts >> Levels;
			Vector2 step = Vector2.Zero;
			Vector2 start = -Extents;

			for ( int x = 0; x < MaxResolution + 1; x++ )
			{
				step.x = stepSize * x;
				for ( int y = 0; y < MaxResolution + 1; y++ )
				{
					step.y = stepSize * y;
					Vector2 pos = start + step;

					GridValues[x, y] = 1000000f;
				}
			}

			return GridValues;
		}

		public static void Initialize()
		{
			models = InitializeModels();

			if ( Host.IsClient )
			{
				surfaceMeshes = InitializeMeshes();
				edgeMeshes = InitializeMeshes();
			}

			RootCell = new TreeNode();

			BuildModels( true );
		}

		// Model Count is equal to 4^n.
		public const int ModelLevel = 3;
		private const int modelsPerAxis = 1 << ModelLevel;
		private const int modelCount = modelsPerAxis * modelsPerAxis;
		private const int modelExtents = Extents >> ModelLevel;

		private static ModelEntity[] models;
		private static Mesh[] surfaceMeshes;
		private static Mesh[] edgeMeshes;
		private static Vector3 meshBounds = new Vector3( modelExtents, modelExtents, MarchingSquares.EdgeWidth );

		private static ModelEntity[] InitializeModels()
		{
			ModelEntity[] models = new ModelEntity[modelCount];

			for ( int i = 0; i < modelCount; i++ )
			{
				ModelEntity modelEnt = new ModelEntity();
				modelEnt.Rotation = Rotation.FromRoll( 90f );
				modelEnt.Transmit = TransmitType.Never;
				modelEnt.Tags.Add( "Terrain" );
				models[i] = modelEnt;
			}

			return models;
		}

		private static Mesh[] InitializeMeshes()
		{
			Mesh[] meshes = new Mesh[modelCount];

			int maxVertices = 500000 / modelCount;

			for ( int i = 0; i < modelCount; i++ )
			{
				Mesh mesh = new Mesh( MarchingSquares.Material );
				mesh.CreateVertexBuffer<TerrainVertex>( maxVertices, TerrainVertex.Layout );
				mesh.CreateIndexBuffer( maxVertices );
				mesh.LockVertexBuffer<TerrainVertex>( RebuildBuffer );
				mesh.LockIndexBuffer( RebuildBuffer );
				meshes[i] = mesh;
			}

			return meshes;
		}

		private static void RebuildBuffer( Span<TerrainVertex> vertices ) { }
		private static void RebuildBuffer( Span<int> indices ) { }

		public static void BuildModels( bool forced = false )
		{
			Stopwatch buildwatch = new();
			List<TreeNode> cells = RootCell.CellsAtLevel( ModelLevel );

			int updates = 0;
			int index = 0;
			foreach ( TreeNode node in cells )
			{
				if ( forced || node.Dirty )
				{
					node.Dirty = false;

					node.GetMeshData( out var surfaceData, out var edgeData );

					if ( MarchingSquares.MeshSimplification )
					{
						surfaceData.RemoveDuplicateVertices();
						edgeData.RemoveDuplicateVertices();
					}


					ModelBuilder modelBuilder = new ModelBuilder();

					BBox bounds = new BBox( (Vector3)node.Center - meshBounds, (Vector3)node.Center + meshBounds );

					int surfaceCount = surfaceData.IndexCount;
					if ( surfaceCount > 0 )
					{
						if ( Host.IsClient )
						{
							Mesh surfaceMesh = surfaceMeshes[index];
							UpdateMesh( surfaceMesh, surfaceData );
							surfaceMesh.Bounds = bounds;
							modelBuilder.AddMesh( surfaceMesh );
						}
					}

					int edgeCount = edgeData.IndexCount;
					if ( edgeCount > 0 )
					{
						if ( Host.IsClient )
						{
							Mesh edgeMesh = edgeMeshes[index];
							UpdateMesh( edgeMesh, edgeData );
							edgeMesh.Bounds = bounds;
							modelBuilder.AddMesh( edgeMesh );
						}

						modelBuilder.AddCollisionMesh( edgeData.VertexPositions.ToArray(), edgeData.Indices.ToArray() );
					}
					models[index].Model = modelBuilder.Create();

					if ( edgeCount > 0 )
						models[index].SetupPhysicsFromModel( PhysicsMotionType.Static );

					updates++;
				}

				index++;
			}
		}


		private static void UpdateMesh( Mesh mesh, MeshData meshData )
		{
			mesh.SetIndexRange( 0, meshData.IndexCount );
			mesh.SetIndexBufferData( meshData.Indices );
			mesh.SetVertexRange( 0, meshData.VertexCount );
			mesh.SetVertexBufferData( meshData.Vertices );
		}

		public static void Simulate( Client cl )
		{
			if ( RootCell == null )
				return;

			BuildModels();
		}

		public static void Update( SDF sdf )
		{
			if ( RootCell == null )
				return;

			RootCell.Update( sdf );
		}

		/// <summary>
		/// Debug method for testing meshes.
		/// </summary>
		[ClientCmd( "meshtest" )]
		public static void MeshTest()
		{
			List<TreeNode> nodes = RootCell.CellsAtLevel( ModelLevel );
			foreach ( TreeNode node in nodes )
			{
				Log.Info( "New Node" );
				node.GetMeshData( out MeshData surfaceData, out MeshData edgeData );
				surfaceData.RemoveDuplicateVertices();
				edgeData.RemoveDuplicateVertices();

				surfaceData.DrawDebug( 5f, Color.White );

				Log.Info( $"surfaceData ({surfaceData.VertexCount}, {surfaceData.IndexCount})" );
			}
		}

		public static void DrawGrid()
		{
			float stepSize = 4 << ExtentShifts >> Levels;
			Vector3 step = Vector2.Zero;
			Vector3 start = new Vector3( -Extents, -Extents, 0f );

			Vector3 upward = new Vector3( 0, 0, MarchingSquares.EdgeWidth * 0.5f + 1f );

			Rotation up = Rotation.FromPitch( 90f );

			for ( int x = 0; x < MaxResolution + 1; x++ )
			{
				step.x = stepSize * x;
				for ( int y = 0; y < MaxResolution + 1; y++ )
				{
					step.y = stepSize * y;
					Vector3 pos = start + step;

					float rgb = 1f - Math.Clamp( GridValues[x, y] * 0.01f, 0f, 1f );
					rgb *= rgb;

					if ( rgb <= 0f )
						continue;

					Color color = new Color( rgb );

					DebugOverlay.Circle( pos + upward, up, stepSize * 0.375f, color );

					DebugOverlay.Text( pos, $"{GridValues[x, y]}", Color.White, 0, 256f );
				}
			}
		}

		/// <summary>
		/// Debug method for cleaning the quadtree.
		/// </summary>
		[ClientCmd( "cleantree" )]
		public static void CleanTree()
		{
			RootCell.Clean();
		}
	}
}
