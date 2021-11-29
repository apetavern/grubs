using Sandbox;
using System.Linq;
using Sandbox.UI;
using System;
using System.Collections.Generic;

public partial class TerrainChunk : ModelEntity
{
	private static int resolution => Terrain.ChunkResolution;
	private static int width => Terrain.ChunkWidth;
	private static int extents => width / 2;

	public static Vector3 ToWorld( int x, int y )
	{
		return new Vector3( width * (Terrain.ChunksWide / 2 - x), 0, width * (Terrain.ChunksHigh / 2 - y) );
	}

	private static Vector3[] pointOffsets = GeneratePointOffsets();

	[Net] public int X { get; private set; }
	[Net] public int Y { get; private set; }
	public Mesh Mesh { get; private set; }
	public static TerrainChunk Create( int x, int y )
	{
		TerrainChunk chunk = new TerrainChunk() { X = x, Y = y };
		chunk.Position = ToWorld( x, y );

		return chunk;
	}

	public TerrainChunk() { }

	private void Initalize()
	{
		PhysicsEnabled = false;

		Mesh = new Mesh( MarchingCubes.Material );
		Mesh.CreateVertexBuffer<EdgeVertex>( 2000, EdgeVertex.Layout );
		Mesh.LockVertexBuffer<EdgeVertex>( Rebuild );

		Generate();
	}

	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Always;

		Initalize();
	}

	public override void ClientSpawn()
	{
		base.ClientSpawn();

		Initalize();
	}

	[Event.Frame]
	public void Frame()
	{
		DebugOverlay.Box( Position, -extents, extents, Color.White.WithAlpha( 0.1f ) );
	}

	public void Generate()
	{
		UpdateMesh( out Vector3[] vertices, out int[] indices );

		if ( indices.Length == 0 )
		{
			EnableDrawing = false;
			EnableAllCollisions = false;
			return;
		}

		ModelBuilder modelBuilder = new ModelBuilder();
		if ( Host.IsClient )
			modelBuilder.AddMesh( Mesh );

		modelBuilder.AddCollisionMesh( vertices, indices );

		SetModel( modelBuilder.Create() );
		SetupPhysicsFromModel( PhysicsMotionType.Static, false );

		EnableDrawing = true;
		EnableAllCollisions = true;

		if ( IsServer )
			ClientGenerate();
	}

	[ClientRpc]
	public void ClientGenerate()
	{
		Generate();
	}

	private static void Rebuild( Span<EdgeVertex> vertices ) { }

	public void UpdateMesh( out Vector3[] vertArray, out int[] indexArray )
	{
		List<Vector3> vertList = new List<Vector3>();
		List<int> indexList = new List<int>();

		int indices = 0;

		List<EdgeVertex> vertices = new List<EdgeVertex>();

		float[] cubeNoises = new float[8];

		Vector3[] edgePoints = new Vector3[12];

		float sizeStep = width / (float)resolution;

		int cX = (Terrain.ChunksWide - (X + 1)) * resolution;
		int cY = (Terrain.ChunksHigh - (Y + 1)) * resolution;

		Vector3 center = Vector3.Zero;
		for ( int x = 0; x < resolution; x++ )
		{
			center.x = x * sizeStep;
			for ( int y = 0; y < 2; y++ )
			{
				//timeOffset = Sinterp.GetRaw( Time.Now + (y+x) * 0.125f ) * 16f;
				center.y = y * sizeStep;
				for ( int z = 0; z < resolution; z++ )
				{
					center.z = z * sizeStep;

					int cubeByte = 0;
					for ( int i = 0; i < 8; i++ )
					{
						int nX = MarchingCubes.PointOffsets[i, 0];
						int nY = MarchingCubes.PointOffsets[i, 1];
						int nZ = MarchingCubes.PointOffsets[i, 2];

						float noise = Terrain.NoiseMap[cX + x + nX, y + nY, cY + z + nZ];

						cubeNoises[i] = noise;
						if ( noise > MarchingCubes.GroundLevel )
							cubeByte += 1 << i;
					}

					int edgeFlags = MarchingCubes.EdgeFlags[cubeByte];
					if ( edgeFlags == 0 ) continue; // skip cube if no intersections

					int triangleCount = MarchingCubes.TriangleCount[cubeByte];

					//Vector3 up = (ChunkPosition + center).Normal;

					for ( int i = 0; i < 12; i++ ) // for each edge
					{
						if ( (edgeFlags & (1 << i)) != 0 ) // if it has intersection
						{
							int a = MarchingCubes.IndexFromEdge[i, 0];
							int b = MarchingCubes.IndexFromEdge[i, 1];

							float aNoise = cubeNoises[a];
							float bNoise = cubeNoises[b];

							float interp = (MarchingCubes.GroundLevel - aNoise) / (bNoise - aNoise);

							Vector3 aCorner = pointOffsets[a];
							Vector3 bCorner = pointOffsets[b];

							Vector3 edgeSmooth = aCorner + interp * (bCorner - aCorner);

							edgePoints[i] = center + edgeSmooth;
						}
					}

					for ( int t = 0; t < triangleCount; t += 3 )
					{
						Vector3[] verts = new Vector3[3];

						int a = MarchingCubes.Triangles[cubeByte, t];
						int b = MarchingCubes.Triangles[cubeByte, t + 1];
						int c = MarchingCubes.Triangles[cubeByte, t + 2];

						verts[0] = edgePoints[a];
						verts[1] = edgePoints[b];
						verts[2] = edgePoints[c];

						Vector3 tangent = (verts[1] - verts[0]);
						Vector3 normal = (verts[1] - verts[2]).Cross( tangent );

						for ( int i = 0; i < 3; i++ )
						{
							Vector3 pos = verts[i] - extents;

							if ( y == 2 )
							{
								pos.y += 80f;
							}
							else if ( y == 1 && verts[i].y == 32 )
							{
								pos.y += 80f;
							}



							float cT = 1f - (32f - verts[i].y) * 0.1f;

							vertices.Add( new EdgeVertex( pos, normal, dirtColor.ToColor().Darken( cT.Clamp( 0f, 0.35f ) ) ) );
							vertList.Add( pos );
							indexList.Add( indices );
							indices++;
						}
					}
				}
			}
		}

		vertArray = vertList.ToArray();
		indexArray = indexList.ToArray();

		if ( vertices.Count == 0 )
			return;

		Mesh.Bounds = new BBox( -extents, extents );
		Mesh.SetVertexRange( 0, vertices.Count );
		Mesh.SetVertexBufferData<EdgeVertex>( vertices.ToArray() );
	}

	private Color32 grassColor = new Color32( 56, 115, 56 );
	private Color32 dirtColor = new Color32( 115, 62, 57 );

	static Vector3[] GeneratePointOffsets()
	{
		Vector3[] pointOffsets = new Vector3[8];
		float sizeStep = width / (float)resolution;


		for ( int i = 0; i < 8; i++ )
		{
			Vector3 offset = new Vector3(
				MarchingCubes.PointOffsets[i, 0] * sizeStep,
				MarchingCubes.PointOffsets[i, 1] * sizeStep,
				MarchingCubes.PointOffsets[i, 2] * sizeStep
			);
			pointOffsets[i] = offset;
		}
		return pointOffsets;
	}
}
