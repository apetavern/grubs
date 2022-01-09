using Sandbox;
using System;
using System.Collections.Generic;

namespace Grubs.Terrain.Triangulation
{
    public static class TriangulationHelper
	{
		public readonly static Vector3[] cornerVertices = new Vector3[]
		{
				new Vector3(0, 0, 0),
				new Vector3(0, 0, 1),
				new Vector3(1, 0, 1),
				new Vector3(1, 0, 0)
		};

		public static List<Vector3>[] Vertices { get; private set; }
		public static List<int>[] Indices { get; private set; }
		public static Vector3[][] VerticesAsArray { get; private set; }
		public static int[][] IndicesAsArray { get; private set; }
		public static Mesh[] Meshes { get; private set; }
		public static Model[] Models { get; private set; }

		public static void BuildTriangulationTable()
		{
			Vertices = new List<Vector3>[1 << 4];
			Indices = new List<int>[1 << 4];
			VerticesAsArray = new Vector3[1 << 4][];
			IndicesAsArray = new int[1 << 4][];

			for (int marchingID = 0; marchingID <= 0b1111; marchingID++ )
			{
				//Grab the value of the first bit
				bool firstBit = (marchingID & 1) == 1;
				//This is the list of vertices which make up the face of the march
				List<Vector3> faceVerticies = new List<Vector3>();

				int marchingIDCopy = marchingID;
				for ( int i = 0; i < 4; i++ )
				{
					//Get if the current corner is set
					bool set = (marchingIDCopy & 1) == 1;
					//if it is set, add the vertex to the list
					if ( set )
					{
						faceVerticies.Add( cornerVertices[i] );
					}

					//Calculate if the next corner is set
					bool nextIsSet;
					if ( i == 3 )
					{
						nextIsSet = firstBit;
					}
					else
					{
						nextIsSet = ((marchingIDCopy >> 1) & 1) == 1;
					}

					//If one corner is set and next isn't, then we need to add the midpoint
					if ( set != nextIsSet )
					{
						int nextIndex = i == 3 ? 0 : i + 1;
						//Midpoint between the current corner and the next one
						Vector3 midPoint = (cornerVertices[i] + cornerVertices[nextIndex]) / 2;
						faceVerticies.Add( midPoint );
					}
					marchingIDCopy >>= 1;
				}

				//Triangulate the face
				List<int> faceIndicies = triangulateConvexPolygon(faceVerticies);

				List<Vector3> extrusionVertices = new List<Vector3>();
				//Now we need to add the extrusion
				//Since faceVertices is in clockwise order, we can simply go through, and if either the current
				//vertex, the next vertex, or both verices, have a component on 1/2, we add it to the list

				//Note: this step will end up adding a lot of extra vertices to the index buffer that don't necessarily have to exist
				//This could be cleaned up later.
				for(int i = 0; i < faceVerticies.Count; i++ )
				{
					Vector3 thisVertex = faceVerticies[i];
					int nextIndex;
					if( i == faceVerticies.Count - 1 )
					{
						nextIndex = 0;
					}
					else
					{
						nextIndex = i + 1;
					}
					Vector3 nextVertex = faceVerticies[nextIndex];
					if(Vector3HasHalfComponent(thisVertex) && Vector3HasHalfComponent(nextVertex))
					{
						extrusionVertices.Add(thisVertex);
						extrusionVertices.Add(thisVertex + new Vector3(0, 1, 0));
						extrusionVertices.Add( nextVertex );
						extrusionVertices.Add( nextVertex + new Vector3(0, 1, 0));
					}
				}

				List<int> extrusionIndices = new List<int>();

				//Now, take each quad of vertices that need extruding and turn them into an index list
				for(int i = 0; i < extrusionVertices.Count; i += 4 )
				{
					extrusionIndices.Add( i );
					extrusionIndices.Add( i + 3 );
					extrusionIndices.Add( i + 1 );

					extrusionIndices.Add( i );
					extrusionIndices.Add( i + 2);
					extrusionIndices.Add( i + 3 );
				}
				
				//Merge everything back together
				List<Vector3> mergedVertices = new List<Vector3>();
				faceVerticies.Reverse(); //I'm not sure why I need to reverse here.
				mergedVertices.AddRange( faceVerticies );
				mergedVertices.AddRange( extrusionVertices );

				List<int> mergedIndices = new List<int>();
				mergedIndices.AddRange( faceIndicies );
				//Since the extrusion vertices are shifted to the end of the list, we can't just use the extrusionIndices
				//We need to add a value which matches how much the extrusionVertices have been shifted
				int shiftAmount = faceVerticies.Count;
				for(int i = 0; i < extrusionIndices.Count; i++ )
				{
					mergedIndices.Add( extrusionIndices[i] + shiftAmount );
				}

				//Multiple the sizes here so we don't have to do it dynamically
				/*
				for(int i = 0; i < mergedVertices.Count; i++ )
				{
					mergedVertices[i] *= TerryForm.Game.marchSize;
				}
				*/

				//Now, there are duplicate vertices in the list, so we going to compress the indicies of the same verts into one
				int nextAvailableIndex = 0;
				List<Vector3> newMergedVertices = new List<Vector3>();
				List<int> newMergedIndices = new List<int>();
				Dictionary<Vector3, int> Vector3ToIndex = new Dictionary<Vector3, int>();
				//Fill up a dictionary and a list of all the unique vertices
				for(int i = 0; i < mergedVertices.Count; i++ )
				{
					Vector3 thisVertex = mergedVertices[i];
					if(!Vector3ToIndex.ContainsKey( thisVertex ) )
					{
						Vector3ToIndex[thisVertex] = nextAvailableIndex;
						nextAvailableIndex++;
						newMergedVertices.Add( thisVertex );
					}
				}

				//Now go through the mergedIndices list and fill them with the newly compressed vertex indices.
				foreach(int index in mergedIndices)
				{
					Vector3 correspondingIndexVertex = mergedVertices[index];
					newMergedIndices.Add( Vector3ToIndex[correspondingIndexVertex] );
				}

				Vertices[marchingID] = newMergedVertices;
				Indices[marchingID] = newMergedIndices;
				VerticesAsArray[marchingID] = newMergedVertices.ToArray();
				IndicesAsArray[marchingID] = newMergedIndices.ToArray();

			}
			PreProcessMeshes();
		}

		private static void PreProcessMeshes()
		{
			Meshes = new Mesh[1 << 4];
			for ( int marchingID = 0; marchingID <= 0b1111; marchingID++ )
			{
				Mesh mesh = new Mesh( Material.Load( "materials/dev/reflectivity_30.vmat" ) ); //Create a mesh
				mesh.CreateVertexBuffer<SimpleVertex>( 32 * 20, SimpleVertex.Layout ); //32 is an overly-pessimistic estimate
				mesh.CreateIndexBuffer( 32 * 3 ); //Likewise, this is also overly pessimistic.
				mesh.LockVertexBuffer<SimpleVertex>( RebuildBuffer );
				mesh.LockIndexBuffer( RebuildIndexBuffer );

				//Grab the relavent verts and indicies
				List<Vector3> vertices = TriangulationHelper.Vertices[marchingID];
				List<int> indices = TriangulationHelper.Indices[marchingID];

				//if there are no vertices, then we don't need a mesh.
				if ( vertices.Count > 0 )
				{
					//Create a vertex array, and fill it will the vertices.
					SimpleVertex[] simpleVertices = new SimpleVertex[vertices.Count];
					for ( int i = 0; i < vertices.Count; i++ )
					{
						simpleVertices[i] = new SimpleVertex()
						{
							position = vertices[i],
							normal = Vector3.Up,
							tangent = Vector3.Forward,
							texcoord = Vector3.Zero
						};
					}

					//Note: The server only needs a collision mesh, but I'm going to generate both here.

					//Mesh mesh = _meshMap[x, y];
					mesh.SetVertexBufferData<SimpleVertex>( simpleVertices );
					mesh.SetIndexRange( 0, indices.Count );
					mesh.SetIndexBufferData( indices );
					Meshes[marchingID] = mesh;
				}
				else
				{
					Meshes[marchingID] = null;
					//Model model = new ModelBuilder().Create();
					//modelEntity.SetModel( model );
				}
			}
			PreprocessModels();
		}

		public static void PreprocessModels()
		{
			Models = new Model[1 << 4];
			for ( int marchingID = 0; marchingID <= 0b1111; marchingID++ )
			{
				Mesh mesh = TriangulationHelper.Meshes[marchingID];
				Model model;
				if ( mesh != null )
				{
					model = new ModelBuilder()
						.AddMesh( mesh )
						.AddCollisionMesh( TriangulationHelper.VerticesAsArray[marchingID], TriangulationHelper.IndicesAsArray[marchingID] )
						.Create();
				}
				else
				{
					model = new ModelBuilder().Create();
				}
				Models[marchingID] = model;
			}
		}

		private static void RebuildBuffer( Span<SimpleVertex> buffer )
		{
			//I don't know why this needs to exist!
		}
		private static void RebuildIndexBuffer( Span<int> buffer )
		{
			//I don't know why this needs to exist!
		}

		private static bool Vector3HasHalfComponent(Vector3 vector)
		{
			return vector.x == 0.5f || vector.y == 0.5f || vector.z == 0.5f;
		}

		//Returns a list of integers which creates the minimum weight triangulation for an input set of vertices
		private static List<int> triangulateConvexPolygon( List<Vector3> vertices )
		{
			List<int> vertexIndices = new List<int>();
			(double weight, List<int> vertexIndices) result = mwt( 0, vertices.Count - 1, vertices );
			return result.vertexIndices;
		}

		//Recursive brute force solution for "minimumw weight triangulation"
		private static (double weight, List<int> vertexIndices) mwt( int i, int j, List<Vector3> vertices )
		{
			//It cannot be triangulated if there are less than 3 vertices
			if ( j >= i + 2 )
			{
				float ijWeight = vertices[i].Distance( vertices[j] );

				(double weight, List<int> vertices) minimumWeightSet = (double.PositiveInfinity, null);

				for ( int k = i + 1; k < j; k++ )
				{
					double thisTriangleWeight = vertices[i].Distance( vertices[k] ) + vertices[k].Distance( vertices[j] ) + vertices[j].Distance( vertices[i] );
					(double weight, List<int> vertexIndices) ikRecursion = mwt( i, k, vertices );

					(double weight, List<int> vertexIndices) kjRecursion = mwt( k, j, vertices );

					double totalWeight = thisTriangleWeight + ikRecursion.weight + kjRecursion.weight;
					if ( totalWeight <= minimumWeightSet.weight )
					{
						List<int> newIndices = new List<int>() { i, k, j };
						newIndices.AddRange( ikRecursion.vertexIndices );
						newIndices.AddRange( kjRecursion.vertexIndices );

						minimumWeightSet = (totalWeight, newIndices);
					}
				}
				return minimumWeightSet;
			}
			else
			{
				return (0, new List<int>());
			}
		}
	}
}
