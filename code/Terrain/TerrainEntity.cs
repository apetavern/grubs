using Sandbox;
using System;
using System.Collections.Generic;
using Grubs.Terrain.SDFs;
using Grubs.Terrain.Triangulation;

namespace Grubs.Terrain
{
	public partial class TerrainEntity : Entity, IDestructableTerrain
	{
		//The origin position
		[Net] public Vector3 origin { get; private set; }
		//The total width and height from the origin
		[Net] public float size { get; private set; }
		//Networked list of SDFs
		private int lastUsedSDF;
		[Net] private IList<SDF> SDFs { get; set; }
		private Quadtree quadtree;

		public TerrainEntity()
		{
			quadtree = new Quadtree();

			lastUsedSDF = -1;
		}

		public TerrainEntity( Vector3 origin, float size ) : this()
		{
			this.origin = origin;
			this.size = size;
		}

		public void AddSDF( SDF sdf )
		{
			SDFs.Add( sdf );
		}

		public void rebuildFromScratch()
		{
			//Print some basic data
			Log.Info( "Rebuilding" );
			Log.Info( $"Center: {Position}" );
			Log.Info( $"Size: {this.size}" );
			foreach ( SDF sdf in SDFs )
			{
				Log.Info( sdf );
			}
			//End printing data

			//Clear all of the existing map objects.
			quadtree.deleteEntities();
			quadtree = new Quadtree();
			//End clearing map data


			lastUsedSDF = -1;
			while ( lastUsedSDF < SDFs.Count - 1 )
			{
				considerNextSDF();
			}
		}

		public void considerNextSDFClientAndServer()
		{
			considerNextSDF();
			considerNextSDFClient();
		}

		//This function will modify the current map based on the next SDF
		public void considerNextSDF()
		{
			SDF nextSDF = SDFs[lastUsedSDF + 1];
			lastUsedSDF++;
			Log.Info( nextSDF );

			//CircleSDF csdf = (CircleSDF)nextSDF;
			//DebugOverlay.Circle( new Vector3( csdf._position.x, origin.y, csdf._position.y ), Rotation.Identity.RotateAroundAxis(Vector3.Up, 90), csdf._radius, Color.Blue, true, 5f );

			recursiveSDFHelper( nextSDF, new Vector2( origin.x, origin.z ), size, 0, quadtree );
		}

		[ClientRpc]
		public void considerNextSDFClient()
		{
			considerNextSDF();
		}

		private void recursiveSDFHelper( SDF sdf, Vector2 position, float size, int depth, Quadtree node )
		{
			//Log.Info( sdf.GetDistance( new Vector2(-128, 512) ) );
			//DebugOverlay.Sphere( new Vector3( position.x, origin.y, position.y ), size/2, Color.Blue, true, 10.0f );
			(float v00, float v01, float v10, float v11) values = getCornerValues( sdf, position, size );
			if ( sdf.ModifyType == ModifyType.FILL )
			{
				node.SDF00 = MathF.Min( node.SDF00, values.v00 );
				node.SDF01 = MathF.Min( node.SDF01, values.v01 );
				node.SDF10 = MathF.Min( node.SDF10, values.v10 );
				node.SDF11 = MathF.Min( node.SDF11, values.v11 );
			}
			else
			{
				node.SDF00 = MathF.Max( node.SDF00, -values.v00 );
				node.SDF01 = MathF.Max( node.SDF01, -values.v01 );
				node.SDF10 = MathF.Max( node.SDF10, -values.v10 );
				node.SDF11 = MathF.Max( node.SDF11, -values.v11 );
			}

			ContainmentStatus status = getContainmentStatus( position, size, sdf );
			if ( depth == 3 )
			{
				//March this node.

				node.deleteEntities();
				//node.ClearChildren();
				//node.ContainmentStatus = NodeStatus.Empty;

				//Now that we have hit the depth limit, we still want more data, so we will begin our chunked marches.
				//To do this, our function will perform very similar operations as this one, but it will return a model
				//as a result of the recursive tree-building
				//node.Entity = GenerateModelEntity( node, size, position );
				node.Entity = GenerateModelEntityRecursive( sdf, node, size, position );

				//DebugOverlay.Box( new Vector3( position.x, origin.y, position.y ) - (new Vector3( 1, 0, 1 ) * size / 2), new Vector3( position.x, origin.y, position.y ) + (new Vector3( 1, 0, 1 ) * size / 2), Color.Yellow, 5 );

				return;
			}

			switch ( status )
			{
				case ContainmentStatus.CompletelyInside:
					node.deleteEntities();
					node.ClearChildren();
					if ( sdf.ModifyType == ModifyType.FILL )
					{
						//node.SDF00 = node.SDF01 = node.SDF10 = node.SDF11 = -1.0f;
						node.Entity = GenerateModelEntity( node, size, position );
						node.ContainmentStatus = NodeStatus.Solid;

						//DebugOverlay.Box( new Vector3( position.x, origin.y, position.y ) - (new Vector3( 1, 0, 1 ) * size / 2), new Vector3( position.x, origin.y, position.y ) + (new Vector3( 1, 0, 1 ) * size / 2), Color.Red, 5 );
					}
					else
					{
						node.ContainmentStatus = NodeStatus.Empty;

						//DebugOverlay.Box( new Vector3( position.x, origin.y, position.y ) - (new Vector3( 1, 0, 1 ) * size / 2), new Vector3( position.x, origin.y, position.y ) + (new Vector3( 1, 0, 1 ) * size / 2), Color.Green, 5 );
					}
					break;
				case ContainmentStatus.CompletelyOutside:
					//Nodes shouldnt be affected if they are completely outside.
					//DebugOverlay.Box( new Vector3( position.x, origin.y, position.y ) - (new Vector3( 1, 0, 1 ) * size / 2), new Vector3( position.x, origin.y, position.y ) + (new Vector3( 1, 0, 1 ) * size / 2), Color.White, 5 );
					break;
				case ContainmentStatus.Unsure:
					//DebugOverlay.Box( new Vector3( position.x, origin.y, position.y ) - (new Vector3( 1, 0, 1 ) * size / 2), new Vector3( position.x, origin.y, position.y ) + (new Vector3( 1, 0, 1 ) * size / 2), Color.Yellow, 5 );

					float newSize = size / 2;
					float offsetDistance = size / 4;
					//Offsets to the centers of each recursion
					Vector2 offset00 = new Vector2( -offsetDistance, -offsetDistance );
					Vector2 offset01 = new Vector2( -offsetDistance, offsetDistance );
					Vector2 offset10 = new Vector2( offsetDistance, -offsetDistance );
					Vector2 offset11 = new Vector2( offsetDistance, offsetDistance );

					if ( node.Entity != null ) { node.Entity.DeleteAsync( 0.0f ); node.Entity = null; }
					if ( node.ContainmentStatus != NodeStatus.Children )
					{
						float newSDF;
						node.CreateChildren();
						NodeStatus newStatus;
						if ( node.ContainmentStatus == NodeStatus.Complex )
						{
							newStatus = NodeStatus.Empty;
							newSDF = 1.0f;
						}
						else
						{
							newStatus = node.ContainmentStatus;
							newSDF = node.ContainmentStatus == NodeStatus.Solid ? -1.0f : 1.0f;
						}

						node.Child00.SDF00 = node.Child00.SDF01 = node.Child00.SDF10 = node.Child00.SDF11 =
						node.Child01.SDF00 = node.Child01.SDF01 = node.Child01.SDF10 = node.Child01.SDF11 =
						node.Child10.SDF00 = node.Child10.SDF01 = node.Child10.SDF10 = node.Child10.SDF11 =
						node.Child11.SDF00 = node.Child11.SDF01 = node.Child11.SDF10 = node.Child11.SDF11 = newSDF;

						node.Child00.ContainmentStatus = newStatus;
						node.Child01.ContainmentStatus = newStatus;
						node.Child10.ContainmentStatus = newStatus;
						node.Child11.ContainmentStatus = newStatus;
						//I don't get why this is necessary, cant we just delete the entities, and recursiveSDFHelper below will fix it?
						node.Child00.Entity = GenerateModelEntity( node.Child00, newSize, position + offset00 );
						node.Child01.Entity = GenerateModelEntity( node.Child01, newSize, position + offset01 );
						node.Child10.Entity = GenerateModelEntity( node.Child10, newSize, position + offset10 );
						node.Child11.Entity = GenerateModelEntity( node.Child11, newSize, position + offset11 );
					}
					//node.deleteEntities();
					node.ContainmentStatus = NodeStatus.Children;

					recursiveSDFHelper( sdf, position + offset00, newSize, depth + 1, node.Child00 );
					recursiveSDFHelper( sdf, position + offset01, newSize, depth + 1, node.Child01 );
					recursiveSDFHelper( sdf, position + offset10, newSize, depth + 1, node.Child10 );
					recursiveSDFHelper( sdf, position + offset11, newSize, depth + 1, node.Child11 );
					break;
			}
		}

		private ModelEntity GenerateModelEntityRecursive( SDF sdf, Quadtree node, float size, Vector2 position )
		{
			return GenerateModelEntityRecursive( sdf, node, size, position, 0 );
		}

		private ModelEntity GenerateModelEntityRecursive( SDF sdf, Quadtree node, float size, Vector2 position, int startDepth )
		{
			(List<Vector3> vertices, List<int> indices) result = GenerateVerticesRecursive( sdf, node, size, position, startDepth );

			if ( result.vertices.Count > 0 )
			{
				Mesh mesh = new Mesh( Material.Load( "materials/dev/reflectivity_30.vmat" ) ); //Create a mesh
				SimpleVertex[] simpleVertices = new SimpleVertex[result.vertices.Count];
				for ( int i = 0; i < result.vertices.Count; i++ )
				{
					simpleVertices[i] = new SimpleVertex()
					{
						position = result.vertices[i],
						normal = Vector3.Up,
						tangent = Vector3.Forward,
						texcoord = Vector3.Zero
					};
				}
				mesh.CreateVertexBuffer<SimpleVertex>( result.vertices.Count, SimpleVertex.Layout, simpleVertices );
				mesh.CreateIndexBuffer( result.indices.Count, result.indices );

				Model model = new ModelBuilder()
					.AddMesh( mesh )
					.AddCollisionMesh( result.vertices.ToArray(), result.indices.ToArray() )
					.Create();
				ModelEntity modelEntity = new ModelEntity();
				modelEntity.Model = model;
				modelEntity.Spawn();
				modelEntity.SetupPhysicsFromModel( PhysicsMotionType.Static );
				modelEntity.Position = new Vector3( position.x, origin.y, position.y ) - new Vector3( size / 2, 0, size / 2 );
				modelEntity.Transmit = TransmitType.Always;
				return modelEntity;
			}
			else
			{
				return null;
			}
		}

		private (List<Vector3> vertices, List<int> indices) GenerateVerticesRecursive( SDF sdf, Quadtree node, float size, Vector2 position, int depth)
		{
			(float v00, float v01, float v10, float v11) values = getCornerValues( sdf, position, size );
			if ( sdf.ModifyType == ModifyType.FILL )
			{
				node.SDF00 = MathF.Min( node.SDF00, values.v00 );
				node.SDF01 = MathF.Min( node.SDF01, values.v01 );
				node.SDF10 = MathF.Min( node.SDF10, values.v10 );
				node.SDF11 = MathF.Min( node.SDF11, values.v11 );
			}
			else
			{
				node.SDF00 = MathF.Max( node.SDF00, -values.v00 );
				node.SDF01 = MathF.Max( node.SDF01, -values.v01 );
				node.SDF10 = MathF.Max( node.SDF10, -values.v10 );
				node.SDF11 = MathF.Max( node.SDF11, -values.v11 );
			}
			ContainmentStatus status = getContainmentStatus( position, size, sdf );

			if ( depth == 6 ) //If we are at the recursion limit
			{
				node.deleteEntities();
				node.ClearChildren();
				byte marchingID = GetMarchingID( node );

				node.ContainmentStatus = NodeStatus.Complex;
				setNodeToID( marchingID, node );

				//DebugOverlay.Box( new Vector3( position.x, origin.y, position.y ) - (new Vector3( 1, 0, 1 ) * size / 2), new Vector3( position.x, origin.y, position.y ) + (new Vector3( 1, 0, 1 ) * size / 2), Color.Orange, 5 );

				List<Vector3> vertices = new List<Vector3>();
				TriangulationHelper.Vertices[marchingID].ForEach( v => vertices.Add( v * size ) );
				node.meshData = (vertices, TriangulationHelper.Indices[marchingID]);
				return node.meshData;
			}
			switch ( status )
			{
				case ContainmentStatus.CompletelyInside:
					node.deleteEntities();
					node.ClearChildren();
					{
						byte marchingID = GetMarchingID( node );
						if ( sdf.ModifyType == ModifyType.FILL )
						{
							node.ContainmentStatus = NodeStatus.Solid;
							//DebugOverlay.Box( new Vector3( position.x, origin.y, position.y ) - (new Vector3( 1, 0, 1 ) * size / 2), new Vector3( position.x, origin.y, position.y ) + (new Vector3( 1, 0, 1 ) * size / 2), Color.Red, 5 );

							List<Vector3> vertices = new List<Vector3>();
							TriangulationHelper.Vertices[marchingID].ForEach( v => vertices.Add( v * size ) );
							node.meshData = (vertices, TriangulationHelper.Indices[marchingID]);
							return node.meshData;
						}
						else
						{
							node.ContainmentStatus = NodeStatus.Empty;

							//DebugOverlay.Box( new Vector3( position.x, origin.y, position.y ) - (new Vector3( 1, 0, 1 ) * size / 2), new Vector3( position.x, origin.y, position.y ) + (new Vector3( 1, 0, 1 ) * size / 2), Color.Green, 5 );
							node.meshData = (new List<Vector3>(), new List<int>());
							return node.meshData;
						}
					}
				case ContainmentStatus.CompletelyOutside:
					//Nodes shouldnt be affected if they are completely outside.
					//DebugOverlay.Box( new Vector3( position.x, origin.y, position.y ) - (new Vector3( 1, 0, 1 ) * size / 2), new Vector3( position.x, origin.y, position.y ) + (new Vector3( 1, 0, 1 ) * size / 2), Color.White, 5 );
					//return node.meshData;
					/*
					{
						byte marchingID = GetMarchingID( node );
						List<Vector3> vertices = new List<Vector3>();

						TriangulationHelper.Vertices[marchingID].ForEach( v => vertices.Add( v * size ) );

						return (vertices, TriangulationHelper.Indices[marchingID]);
					}
					*/
					//Let it fall through in this case, because I dont know.


					//return (new List<Vector3>(), new List<int>());
				case ContainmentStatus.Unsure:
					//Log.Info( depth );
					//DebugOverlay.Box( new Vector3( position.x, origin.y, position.y ) - (new Vector3( 1, 0, 1 ) * size / 2), new Vector3( position.x, origin.y, position.y ) + (new Vector3( 1, 0, 1 ) * size / 2), Color.Yellow, 5 );

					float newSize = size / 2;
					float offsetDistance = size / 4;
					//Offsets to the centers of each recursion
					Vector2 offset00 = new Vector2( -offsetDistance, -offsetDistance );
					Vector2 offset01 = new Vector2( -offsetDistance, offsetDistance );
					Vector2 offset10 = new Vector2( offsetDistance, -offsetDistance );
					Vector2 offset11 = new Vector2( offsetDistance, offsetDistance );

					if ( node.Entity != null ) { node.Entity.DeleteAsync( 0.0f ); node.Entity = null; }
					if ( node.ContainmentStatus != NodeStatus.Children )
					{
						//Log.Info( node.ContainmentStatus );
						float newSDF;
						node.CreateChildren();
						NodeStatus newStatus;
						if ( node.ContainmentStatus == NodeStatus.Complex )
						{
							newStatus = NodeStatus.Complex;
							//newSDF = 1.0f;

							bool isNegative00 = node.SDF00 < 0;
							bool isNegative01 = node.SDF01 < 0;
							bool isNegative10 = node.SDF10 < 0;
							bool isNegative11 = node.SDF11 < 0;

							float leftMidpoint = (isNegative00 ^ isNegative10) ? 1 : (isNegative00 ? -1 : 1);
							float rightMidpoint = (isNegative01 ^ isNegative11) ? 1 : (isNegative01 ? -1 : 1);
							float topMidpoint = (isNegative10 ^ isNegative11) ? 1 : (isNegative10 ? -1 : 1);
							float bottomMidpoint = (isNegative00 ^ isNegative01) ? 1 : (isNegative00 ? -1 : 1);

							node.Child00.SDF00 = node.SDF00;
							node.Child01.SDF01 = node.SDF01;
							node.Child10.SDF10 = node.SDF10;
							node.Child11.SDF11 = node.SDF11;

							node.Child00.SDF01 = node.Child01.SDF00 = bottomMidpoint;
							node.Child00.SDF10 = node.Child10.SDF00 = leftMidpoint;
							node.Child10.SDF11 = node.Child11.SDF10 = topMidpoint;
							node.Child11.SDF01 = node.Child01.SDF11 = rightMidpoint;

							node.Child00.SDF11 = node.Child01.SDF10 = node.Child10.SDF01 = node.Child11.SDF00 = 1;
						}
						else
						{
							//Log.Info( "Doing this thing" );
							newStatus = node.ContainmentStatus;
							newSDF = node.ContainmentStatus == NodeStatus.Solid ? -1.0f : 1.0f;
							node.Child00.SDF00 = node.Child00.SDF01 = node.Child00.SDF10 = node.Child00.SDF11 =
							node.Child01.SDF00 = node.Child01.SDF01 = node.Child01.SDF10 = node.Child01.SDF11 =
							node.Child10.SDF00 = node.Child10.SDF01 = node.Child10.SDF10 = node.Child10.SDF11 =
							node.Child11.SDF00 = node.Child11.SDF01 = node.Child11.SDF10 = node.Child11.SDF11 = newSDF;
							if( node.ContainmentStatus == NodeStatus.Solid )
							{
								List<Vector3> vertices = new List<Vector3>();
								TriangulationHelper.Vertices[0b1111].ForEach( v => vertices.Add( v * size ) );
								node.Child00.meshData = 
									node.Child01.meshData = 
									node.Child10.meshData = 
									node.Child11.meshData = (vertices, TriangulationHelper.Indices[0b1111]);
							}
							else
							{
								node.Child00.meshData =
									node.Child01.meshData =
									node.Child10.meshData =
									node.Child11.meshData = (new List<Vector3>(), new List<int>());
							}
						}

						node.Child00.ContainmentStatus = newStatus;
						node.Child01.ContainmentStatus = newStatus;
						node.Child10.ContainmentStatus = newStatus;
						node.Child11.ContainmentStatus = newStatus;
					}
					//node.deleteEntities();
					node.ContainmentStatus = NodeStatus.Children;

					var result00 = GenerateVerticesRecursive( sdf, node.Child00, newSize, position + offset00, depth + 1 );
					var result01 = GenerateVerticesRecursive( sdf, node.Child01, newSize, position + offset01, depth + 1 );
					var result10 = GenerateVerticesRecursive( sdf, node.Child10, newSize, position + offset10, depth + 1 );
					var result11 = GenerateVerticesRecursive( sdf, node.Child11, newSize, position + offset11, depth + 1 );

					if ( (node.Child00.ContainmentStatus == node.Child01.ContainmentStatus &&
						 node.Child00.ContainmentStatus == node.Child10.ContainmentStatus &&
						 node.Child00.ContainmentStatus == node.Child11.ContainmentStatus)
						 &&
						(node.Child00.ContainmentStatus == NodeStatus.Solid ||
						  node.Child00.ContainmentStatus == NodeStatus.Empty) )
					{ //If chilren are all solid or all empty
						node.ContainmentStatus = node.Child00.ContainmentStatus;
						node.ClearChildren();

						if ( node.ContainmentStatus == NodeStatus.Solid )
						{
							List<Vector3> vertices = new List<Vector3>();
							TriangulationHelper.Vertices[0b1111].ForEach( v => vertices.Add( v * size ) );
							node.meshData = (vertices, TriangulationHelper.Indices[0b1111]);
						}
						else
						{
							node.meshData = (new List<Vector3>(), new List<int>());
						}
						return node.meshData;
					}
					else
					{
						//Otherwise we can merge the results
						List<Vector3> newVertices00 = new List<Vector3>();
						result00.vertices.ForEach( e => newVertices00.Add( e ) );
						result00.vertices = newVertices00;

						List<Vector3> newVertices01 = new List<Vector3>();
						result01.vertices.ForEach( e => newVertices01.Add( e ) );
						result01.vertices = newVertices01;

						List<Vector3> newVertices10 = new List<Vector3>();
						result10.vertices.ForEach( e => newVertices10.Add( e ) );
						result10.vertices = newVertices10;

						List<Vector3> newVertices11 = new List<Vector3>();
						result11.vertices.ForEach( e => newVertices11.Add( e ) );
						result11.vertices = newVertices11;

						//Shift the four results accordingly
						float halfSize = size / 2;
						for ( int i = 0; i < result00.vertices.Count; i++ )
						{
							result00.vertices[i] = (result00.vertices[i] + new Vector3( 0, 0, 0 ));
						}

						for ( int i = 0; i < result01.vertices.Count; i++ )
						{
							result01.vertices[i] = (result01.vertices[i] + new Vector3( 0, 0, halfSize ));
						}

						for ( int i = 0; i < result10.vertices.Count; i++ )
						{
							result10.vertices[i] = (result10.vertices[i] + new Vector3( halfSize, 0, 0 ));
						}

						for ( int i = 0; i < result11.vertices.Count; i++ )
						{
							result11.vertices[i] = (result11.vertices[i] + new Vector3( halfSize, 0, halfSize ));
						}
						//Return all 4 results merged together.
						List<Vector3> newVertices = new List<Vector3>();
						newVertices.AddRange( result00.vertices );
						newVertices.AddRange( result01.vertices );
						newVertices.AddRange( result10.vertices );
						newVertices.AddRange( result11.vertices );

						List<int> newIndices = new List<int>();
						int offset = 0;
						result00.indices.ForEach( i => newIndices.Add( i + offset ) );
						offset += result00.vertices.Count;

						result01.indices.ForEach( i => newIndices.Add( i + offset ) );
						offset += result01.vertices.Count;

						result10.indices.ForEach( i => newIndices.Add( i + offset ) );
						offset += result10.vertices.Count;

						result11.indices.ForEach( i => newIndices.Add( i + offset ) );

						//return (result00.vertices, result00.indices);

						node.meshData = (newVertices, newIndices);
						return node.meshData;
						//return MergeVerticeIndiceList( result00, MergeVerticeIndiceList( result01, MergeVerticeIndiceList( result10, result11 ) ) );
					}



			}
			throw new Exception( "WHAT" );
		}

		private static void setNodeToID(byte marchingID, Quadtree node)
		{
			switch(marchingID)
			{
				case 0b0000:
					node.ContainmentStatus = NodeStatus.Empty;
					return;
				case 0b1111:
					node.ContainmentStatus = NodeStatus.Solid;
					return;
				default:
					node.ContainmentStatus = NodeStatus.Complex;
					return;

			}
		}
		private (List<Vector3> vertices, List<int> indices) MergeVerticeIndiceList( (List<Vector3> vertices, List<int> indices) list1, (List<Vector3> vertices, List<int> indices) list2)
		{
			List<Vector3> newVertices = new List<Vector3>(list1.vertices);
			List<int> newIndices = new List<int>(list1.indices);

			newVertices.AddRange( list2.vertices );
			int offset = list1.vertices.Count;
			for(int i = 0; i < list2.indices.Count; i++ )
			{
				newIndices.Add( list2.indices[i] + offset );
			}

			return ( newVertices, newIndices );
		}

		private (float v00, float v01, float v10, float v11) getCornerValues( SDF sdf, Vector2 position, float size )
		{
			float halfSize = size / 2;
			float v00 = sdf.GetDistance( position + new Vector2( -halfSize, -halfSize ) );
			float v01 = sdf.GetDistance( position + new Vector2( -halfSize,  halfSize ) );
			float v10 = sdf.GetDistance( position + new Vector2(  halfSize, -halfSize ) );
			float v11 = sdf.GetDistance( position + new Vector2(  halfSize,  halfSize ) );
			return (v00, v01, v10, v11);
		}
		
		private ContainmentStatus getContainmentStatus(Vector2 position, float size, SDF sdf)
		{
			float radius = MathF.Sqrt( (MathF.Pow( size , 2 ) / 2f ) );
			float distance = sdf.GetDistance( position );
			if( distance < -radius )
			{
				return ContainmentStatus.CompletelyInside;
			}
			else if(distance > radius)
			{
				return ContainmentStatus.CompletelyOutside;
			}
			else
			{
				return ContainmentStatus.Unsure;
			}
		}

		public void display()
		{
			Vector2 position = new Vector2( origin.x, origin.z );
			float size = this.size;
			recursiveDisplay( quadtree, position, size );
		}

		public void recursiveDisplay(Quadtree node, Vector2 position, float size)
		{
			//Log.Info( node.ContainmentStatus );
			switch(node.ContainmentStatus)
			{
				case NodeStatus.Solid:
					DebugOverlay.Box( new Vector3( position.x, origin.y, position.y ) - (new Vector3( 1, 0, 1 ) * size / 2), new Vector3( position.x, origin.y, position.y ) + (new Vector3( 1, 0, 1 ) * size / 2), Color.Red, 5 );
					break;
				case NodeStatus.Empty:
					DebugOverlay.Box( new Vector3( position.x, origin.y, position.y ) - (new Vector3( 1, 0, 1 ) * size / 2), new Vector3( position.x, origin.y, position.y ) + (new Vector3( 1, 0, 1 ) * size / 2), Color.Green, 5 );
					break;
				case NodeStatus.Complex:
					DebugOverlay.Box( new Vector3( position.x, origin.y, position.y ) - (new Vector3( 1, 0, 1 ) * size / 2), new Vector3( position.x, origin.y, position.y ) + (new Vector3( 1, 0, 1 ) * size / 2), Color.Yellow, 5 );
					break;
				case NodeStatus.Children:
					float newSize = size / 2;
					float offsetDistance = size / 4;
					Vector2 offset00 = new Vector2( -offsetDistance, -offsetDistance );
					Vector2 offset01 = new Vector2( -offsetDistance, offsetDistance );
					Vector2 offset10 = new Vector2( offsetDistance, -offsetDistance );
					Vector2 offset11 = new Vector2( offsetDistance, offsetDistance );
					recursiveDisplay( node.Child00, position + offset00, newSize );
					recursiveDisplay( node.Child01, position + offset01, newSize );
					recursiveDisplay( node.Child10, position + offset10, newSize );
					recursiveDisplay( node.Child11, position + offset11, newSize );
					break;
			}
		}

		private byte GetMarchingID(Quadtree node)
		{
			return (byte)(
				(((node.SDF00 < 0) ? 1 : 0) << 0) +
				(((node.SDF01 < 0) ? 1 : 0) << 1) +
				(((node.SDF10 < 0) ? 1 : 0) << 3) +
				(((node.SDF11 < 0) ? 1 : 0) << 2)
			);
		}

		private ModelEntity GenerateModelEntity( Quadtree node, float size, Vector2 position )
		{
			byte marchingID = GetMarchingID( node );
			ModelEntity modelEntity = new ModelEntity();
			modelEntity.Transmit = TransmitType.Always;
			modelEntity.Model = TriangulationHelper.Models[marchingID];
			modelEntity.SetupPhysicsFromModel( PhysicsMotionType.Static );
			//Position + Shift down-left by size/2
			modelEntity.Position = new Vector3( position.x, origin.y, position.y ) - new Vector3( size/2, 0, size/2 );
			modelEntity.Scale = size;
			modelEntity.Spawn();
			return modelEntity;
		}

		public void ShowSDF()
		{
			Vector2 position = new Vector2( origin.x, origin.z );
			recursiveShowSDF( quadtree, position, size );
		}

		private void recursiveShowSDF( Quadtree node, Vector2 position, float size )
		{
			float newSize = size / 2;
			float offsetDistance = size / 2f;
			Vector2 offset00 = new Vector2( -offsetDistance, -offsetDistance );
			Vector2 offset01 = new Vector2( -offsetDistance, offsetDistance );
			Vector2 offset10 = new Vector2( offsetDistance, -offsetDistance );
			Vector2 offset11 = new Vector2( offsetDistance, offsetDistance );
			if (node.ContainmentStatus != NodeStatus.Children)
			{
				Vector3 pos3d = new Vector3( position.x, origin.y, position.y );
				DebugOverlay.Text( pos3d + new Vector3( offset00.x, 0, offset00.y ) / 1.5f, MathF.Floor( node.SDF00 ).ToString(), Color.Yellow, 5.0f, 8000f );
				DebugOverlay.Text( pos3d + new Vector3( offset01.x, 0, offset01.y ) / 1.5f, MathF.Floor( node.SDF01 ).ToString(), Color.Yellow, 5.0f, 8000f );
				DebugOverlay.Text( pos3d + new Vector3( offset10.x, 0, offset10.y ) / 1.5f, MathF.Floor( node.SDF10 ).ToString(), Color.Yellow, 5.0f, 8000f );
				DebugOverlay.Text( pos3d + new Vector3( offset11.x, 0, offset11.y ) / 1.5f, MathF.Floor( node.SDF11 ).ToString(), Color.Yellow, 5.0f, 8000f );
			}
			else
			{
				recursiveShowSDF( node.Child00, position + offset00/2, newSize );
				recursiveShowSDF( node.Child01, position + offset01/2, newSize );
				recursiveShowSDF( node.Child10, position + offset10/2, newSize );
				recursiveShowSDF( node.Child11, position + offset11/2, newSize );
			}
		}

		public void ModifyCircle( Vector2 position, float radius, bool destroy )
		{
			AddSDF( new Circle( position, radius, destroy ? ModifyType.EMPTY : ModifyType.FILL ) );
			considerNextSDFClientAndServer();
		}

		public void ModifyRectangle( Vector2 position, Vector2 size, bool destroy )
		{
			AddSDF( new Rectangle( position, size, destroy ? ModifyType.EMPTY : ModifyType.FILL ) );
			considerNextSDFClientAndServer();
		}

		[Event.Entity.PostSpawn]
		private void OnSpawn()
		{
			rebuildFromScratch();
		}
	}
}
