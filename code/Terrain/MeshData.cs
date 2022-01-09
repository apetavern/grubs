using System.Collections;
using System.Collections.Generic;
using Sandbox;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Grubs.Terrain
{
	public class MeshData
	{
		public List<TerrainVertex> Vertices { get; private set; }
		public List<Vector3> VertexPositions { get; private set; }
		public List<int> Indices { get; private set; }

		public int VertexCount { get; private set; } = 0;
		public int IndexCount { get; private set; } = 0;

		public MeshData()
		{
			Vertices = new List<TerrainVertex>();
			VertexPositions = new List<Vector3>();
			Indices = new List<int>();
		}

		public void AddData( TerrainVertex[] vertices, int[] indices )
		{
			int vertexCount = vertices.Length;
			int indexCount = indices.Length;

			for ( int v = 0; v < vertexCount; v++ )
			{
				TerrainVertex vertex = vertices[v];
				Vertices.Add( vertex );
				VertexPositions.Add( vertex.position );
			}

			for ( int i = 0; i < indexCount; i++ )
				Indices.Add( indices[i] );

			VertexCount += vertexCount;
			IndexCount += indexCount;
		}

		public void RemoveDuplicateVertices( float normalDegrees = 60f )
		{
			if ( VertexCount == 0 )
				return;

			float dot = MathF.Cos( normalDegrees * MathF.PI / 180f );
			//Stopwatch watch = new Stopwatch();

			List<TerrainVertex> newVertices = new List<TerrainVertex>();
			List<Vector3> newPositions = new List<Vector3>();
			List<Vector3> newNormals = new List<Vector3>();
			Dictionary<Vector3, int> newIndices = new();
			List<int> vertexOccurences = new List<int>();

			int newCount = 0;

			for ( int i = 0; i < IndexCount; i++ )
			{
				int index = Indices[i];
				TerrainVertex vertex = Vertices[index];

				Vector3 pos = vertex.position;

				bool exists = newIndices.TryGetValue( pos, out int vertIndex );
				bool sharpEdge = false;

				if ( !exists || (sharpEdge = vertex.normal.Dot( newNormals[vertIndex] ) < dot) )
				{
					Indices[i] = newCount;
					newVertices.Add( vertex );
					newPositions.Add( vertex.position );
					newNormals.Add( vertex.normal );
					vertexOccurences.Add( 1 );

					if ( !sharpEdge )
						newIndices.Add( pos, newCount );
					newCount++;
				}
				else
				{
					Indices[i] = vertIndex;

					if ( pos == Vector3.Zero )
						Log.Error( "Mesh simplification cringed" );

					newNormals[vertIndex] += vertex.normal;
					vertexOccurences[vertIndex]++;
				}
			}

			for ( int i = 0; i < newCount; i++ )
			{
				int occurrences = vertexOccurences[i];
				if ( occurrences == 1 )
					continue;

				Vector3 normal = newNormals[i] / occurrences;
				TerrainVertex vert = Vertices[i];
				vert.normal = normal;
				Vertices[i] = vert;
			}

			//Log.Info( $"Took {watch.Stop()}ms to remove {VertexCount - newCount} duplicate vertices" );

			VertexCount = newCount;
			Vertices = newVertices;
			VertexPositions = newPositions;
		}

		public void DrawDebug( float time = 0f, Color? color = null )
		{
			if ( IndexCount == 0 || VertexCount == 0 )
				return;

			for ( int i = 0; i < IndexCount; i += 3 )
			{
				Color drawColor = color ?? Color.White;

				for ( int j = 0; j < 3; j++ )
				{
					int index = Indices[i + j];
					int nextIndex = Indices[i + (j + 1) % 3];
					Vector3 pos = VertexPositions[index];
					Vector3 nextPos = VertexPositions[nextIndex];

					DebugOverlay.Line( pos, nextPos, drawColor, time );

					//Vector3 normal = Vertices[index].normal;
					//DebugOverlay.Line( pos, pos + normal * 16f, Color.Red, time );
				}
			}
		}
	}
}
