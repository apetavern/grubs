using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Sandbox.Sdf;

internal partial class Sdf3DMeshWriter : Pooled<Sdf3DMeshWriter>, IMeshWriter
{
	private ConcurrentQueue<Triangle> Triangles { get; } = new ConcurrentQueue<Triangle>();
	private Dictionary<VertexKey, int> VertexMap { get; } = new Dictionary<VertexKey, int>();

	public List<Vertex> Vertices { get; } = new List<Vertex>();
	public List<Vector3> VertexPositions { get; } = new List<Vector3>();
	public List<int> Indices { get; } = new List<int>();

	public bool IsEmpty => Indices.Count == 0;

	public byte[] Samples { get; set; }

	public override void Reset()
	{
		Triangles.Clear();
		VertexMap.Clear();

		Vertices.Clear();
		VertexPositions.Clear();
		Indices.Clear();
	}

	private void WriteSlice( in Sdf3DArrayData data, Sdf3DVolume volume, int z )
	{
		var quality = volume.Quality;
		var size = quality.ChunkResolution;

		for ( var y = 0; y < size; ++y )
			for ( var x = 0; x < size; ++x )
				AddTriangles( in data, x, y, z );
	}

	public async Task WriteAsync( Sdf3DArrayData data, Sdf3DVolume volume )
	{
		Triangles.Clear();
		VertexMap.Clear();

		var baseIndex = Vertices.Count;

		var quality = volume.Quality;
		var size = quality.ChunkResolution;

		var tasks = new List<Task>();

		for ( var z = 0; z < size; ++z )
		{
			var zCopy = z;

			tasks.Add( GameTask.RunInThreadAsync( () =>
			{
				WriteSlice( data, volume, zCopy );
			} ) );
		}

		await GameTask.WhenAll( tasks );

		await GameTask.WorkerThread();

		var unitSize = volume.Quality.UnitSize;

		foreach ( var triangle in Triangles )
		{
			Indices.Add( AddVertex( data, triangle.V0, unitSize ) );
			Indices.Add( AddVertex( data, triangle.V1, unitSize ) );
			Indices.Add( AddVertex( data, triangle.V2, unitSize ) );
		}

		for ( var i = baseIndex; i < Vertices.Count; ++i )
		{
			var vertex = Vertices[i];

			Vertices[i] = vertex with { Normal = vertex.Normal.Normal };
		}
	}

	public void ApplyTo( Mesh mesh )
	{
		ThreadSafe.AssertIsMainThread();

		if ( mesh == null )
		{
			return;
		}

		if ( mesh.HasVertexBuffer )
		{
			if ( Indices.Count > 0 )
			{
				if ( mesh.IndexCount < Indices.Count )
				{
					mesh.SetIndexBufferSize( Indices.Count );
				}

				if ( mesh.VertexCount < Vertices.Count )
				{
					mesh.SetVertexBufferSize( Vertices.Count );
				}

				mesh.SetIndexBufferData( Indices );
				mesh.SetVertexBufferData( Vertices );
			}

			mesh.SetIndexRange( 0, Indices.Count );
		}
		else if ( Indices.Count > 0 )
		{
			mesh.CreateVertexBuffer( Vertices.Count, Vertex.Layout, Vertices );
			mesh.CreateIndexBuffer( Indices.Count, Indices );
		}
	}

	private static Vertex GetVertex( in Sdf3DArrayData data, VertexKey key )
	{
		Vector3 pos;

		float xNeg, xPos, yNeg, yPos, zNeg, zPos;

		switch ( key.Vertex )
		{
			case NormalizedVertex.A:
			{
				pos = new Vector3( key.X, key.Y, key.Z );

				xNeg = data[key.X - 1, key.Y, key.Z];
				xPos = data[key.X + 1, key.Y, key.Z];
				yNeg = data[key.X, key.Y - 1, key.Z];
				yPos = data[key.X, key.Y + 1, key.Z];
				zNeg = data[key.X, key.Y, key.Z - 1];
				zPos = data[key.X, key.Y, key.Z + 1];
				break;
			}

			case NormalizedVertex.AB:
			{
				var a = data[key.X, key.Y, key.Z] - 127.5f;
				var b = data[key.X + 1, key.Y, key.Z] - 127.5f;
				var t = a / (a - b);

				pos = new Vector3( key.X + t, key.Y, key.Z );

				xNeg = data[pos.x - 1, key.Y, key.Z];
				xPos = data[pos.x + 1, key.Y, key.Z];
				yNeg = data[pos.x, key.Y - 1, key.Z];
				yPos = data[pos.x, key.Y + 1, key.Z];
				zNeg = data[pos.x, key.Y, key.Z - 1];
				zPos = data[pos.x, key.Y, key.Z + 1];
				break;
			}

			case NormalizedVertex.AC:
			{
				var a = data[key.X, key.Y, key.Z] - 127.5f;
				var c = data[key.X, key.Y + 1, key.Z] - 127.5f;
				var t = a / (a - c);

				pos = new Vector3( key.X, key.Y + t, key.Z );

				xNeg = data[key.X - 1, pos.y, key.Z];
				xPos = data[key.X + 1, pos.y, key.Z];
				yNeg = data[key.X, pos.y - 1, key.Z];
				yPos = data[key.X, pos.y + 1, key.Z];
				zNeg = data[key.X, pos.y, key.Z - 1];
				zPos = data[key.X, pos.y, key.Z + 1];
				break;
			}

			case NormalizedVertex.AE:
			{
				var a = data[key.X, key.Y, key.Z] - 127.5f;
				var e = data[key.X, key.Y, key.Z + 1] - 127.5f;
				var t = a / (a - e);

				pos = new Vector3( key.X, key.Y, key.Z + t );

				xNeg = data[key.X - 1, key.Y, pos.z];
				xPos = data[key.X + 1, key.Y, pos.z];
				yNeg = data[key.X, key.Y - 1, pos.z];
				yPos = data[key.X, key.Y + 1, pos.z];
				zNeg = data[key.X, key.Y, pos.z - 1];
				zPos = data[key.X, key.Y, pos.z + 1];
				break;
			}

			default:
				throw new NotImplementedException();
		}

		return new Vertex( pos, new Vector3( xPos - xNeg, yPos - yNeg, zPos - zNeg ).Normal );
	}

	partial void AddTriangles( in Sdf3DArrayData data, int x, int y, int z );

	private void AddTriangle( int x, int y, int z, CubeVertex v0, CubeVertex v1, CubeVertex v2 )
	{
		Triangles.Enqueue( new Triangle( x, y, z, v0, v1, v2 ) );
	}

	private int AddVertex( in Sdf3DArrayData data, VertexKey key, float unitSize )
	{
		if ( VertexMap.TryGetValue( key, out var index ) )
		{
			return index;
		}

		index = Vertices.Count;

		var vertex = GetVertex( in data, key );

		vertex = vertex with { Position = vertex.Position * unitSize };

		Vertices.Add( vertex );
		VertexPositions.Add( vertex.Position );

		VertexMap.Add( key, index );

		return index;
	}
}
