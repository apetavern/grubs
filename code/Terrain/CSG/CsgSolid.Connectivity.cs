using System;
using System.Collections.Generic;
using System.Linq;

namespace Sandbox.Csg
{
	partial class CsgHull
	{
		internal CsgIsland Island { get; set; }

		internal void AddNeighbors( CsgIsland island, Queue<CsgHull> queue )
		{
			Assert.NotNull( GridCell );

			UpdateNeighbors();

			foreach ( var (neighbor, _) in _neighbors )
			{
				if ( neighbor.GridCell == null )
				{
					Log.Warning( $"Null grid cell: {neighbor.VertexAverage}" );
					continue;
				}

				if ( neighbor.GridCell != GridCell )
				{
					island.NeighborHulls.Add( neighbor );
					continue;
				}

				if ( neighbor.Island == island ) continue;

				Assert.AreEqual( null, neighbor.Island );

				neighbor.Island = island;

				Assert.True( island.Hulls.Add( neighbor ) );

				neighbor.UpdateNeighbors();

				Assert.True( neighbor._neighbors.ContainsKey( this ) );

				queue.Enqueue( neighbor );
			}
		}
	}

	internal class CsgIsland
	{
		public HashSet<CsgHull> Hulls { get; } = new HashSet<CsgHull>();
		public HashSet<CsgHull> NeighborHulls { get; } = new HashSet<CsgHull>();
		public HashSet<CsgIsland> Neighbors { get; } = new HashSet<CsgIsland>();

		public float Volume { get; set; }

		public void Clear()
		{
			foreach ( var neighbor in Neighbors )
			{
				Assert.True( neighbor.Neighbors.Remove( this ) );
			}

			Neighbors.Clear();
			NeighborHulls.Clear();

			foreach ( var hull in Hulls )
			{
				if ( hull.Island == this )
				{
					hull.Island = null;
				}
			}

			Hulls.Clear();

			Volume = 0f;
		}

		[ThreadStatic] private static Queue<CsgHull> _sVisitQueue;

		public void Populate( CsgHull root )
		{
			Assert.AreEqual( null, root.Island );

			CsgHelpers.AssertAreEqual( 0, Hulls.Count );
			CsgHelpers.AssertAreEqual( 0, NeighborHulls.Count );

			var queue = _sVisitQueue ??= new Queue<CsgHull>();
			queue.Clear();

			Hulls.Add( root );
			queue.Enqueue( root );

			root.Island = this;

			while ( queue.TryDequeue( out var next ) )
			{
				Volume += next.Volume;

				next.AddNeighbors( this, queue );
			}
		}
	}

	partial class CsgSolid
	{
		public const float MinVolume = 0.125f;

		private int _nextDisconnectionIndex;

		[Net]
		public bool DisconnectIslands { get; set; } = true;

		[Net]
		public CsgSolid ServerDisconnectedFrom { get; set; }

		public int ClientDisconnectionIndex { get; set; }

		[Net]
		public int ServerDisconnectionIndex { get; set; }

		private bool _copiedInitialGeometry;

		private Dictionary<int, CsgSolid> ClientDisconnections { get; } = new();

		private void CheckInitialGeometry()
		{
			if ( _copiedInitialGeometry || ServerDisconnectedFrom == null ) return;

			if ( ServerDisconnectedFrom.ClientDisconnections.TryGetValue( ServerDisconnectionIndex, out var clientCopy ) )
			{
				ServerDisconnectedFrom.ClientDisconnections.Remove( ServerDisconnectionIndex );

				_copiedInitialGeometry = true;
				_appliedModifications = 0;

				CsgHelpers.AssertAreEqual( _gridSize, clientCopy._gridSize );

				Clear( true );

				foreach ( var pair in clientCopy._grid )
				{
					_grid.Add( pair.Key, pair.Value );

					pair.Value.Solid = this;
					pair.Value.InvalidateMesh();
					pair.Value.InvalidateCollision();
					pair.Value.InvalidateConnectivity();

					foreach ( var hull in pair.Value.Hulls )
					{
						hull.RemoveCollider();
					}
				}

				clientCopy.Clear( false );
				clientCopy.Delete();

				OnModificationsChanged();
			}
		}

		private bool UpdateIslands()
		{
			if ( _invalidConnectivity.Count == 0 ) return false;

			foreach ( var cell in _invalidConnectivity )
			{
				if ( cell.Solid != this ) continue;

				// Clear existing islands for re-use

				foreach ( var island in cell.Islands )
				{
					island.Clear();
				}

				// Populate islands

				var nextIslandIndex = 0;
				var remaining = CsgHelpers.RentHullSet();

				try
				{
					foreach ( var hull in cell.Hulls )
					{
						remaining.Add( hull );
					}

					while ( remaining.Count > 0 )
					{
						var island = cell.GetOrCreateIsland( nextIslandIndex++ );
						var root = remaining.First();

						island.Populate( root );

						Assert.True( island.Hulls.Count > 0 );

						var oldCount = remaining.Count;

						remaining.ExceptWith( island.Hulls );

						CsgHelpers.AssertAreEqual( oldCount - island.Hulls.Count, remaining.Count );
					}
				}
				finally
				{
					CsgHelpers.Return( remaining );
				}

				// Remove empty islands

				for ( var i = cell.Islands.Count - 1; i >= 0; i-- )
				{
					if ( cell.Islands[i].Hulls.Count == 0 )
					{
						cell.Islands.RemoveAt( i );
					}
				}

				// Sort by volume (descending)

				cell.Islands.Sort( ( a, b ) => Math.Sign( b.Volume - a.Volume ) );
			}

			// Update neighbors for changed islands

			foreach ( var cell in _invalidConnectivity )
			{
				if ( cell.Solid != this ) continue;

				foreach ( var island in cell.Islands )
				{
					foreach ( var neighborHull in island.NeighborHulls )
					{
						Assert.NotNull( neighborHull.Island );
						Assert.False( neighborHull.Island == island );

						island.Neighbors.Add( neighborHull.Island );
						neighborHull.Island.Neighbors.Add( island );
					}

					// Don't need these any more

					island.NeighborHulls.Clear();
				}

				cell.PostConnectivityUpdate();
			}

			_invalidConnectivity.Clear();

			return true;
		}

		[ThreadStatic]
		private static HashSet<CsgIsland> _sIslandSet;
		[ThreadStatic]
		private static Queue<CsgIsland> _sIslandQueue;
		[ThreadStatic]
		private static List<(CsgIsland Root, int Count, float Volume)> _sChunks;

		public bool Deleted { get; private set; }

		private bool ConnectivityUpdate()
		{
			// We don't want to kill off disconnected islands.
			return false;

			if ( !DisconnectIslands ) return false;
			if ( !UpdateIslands() ) return false;


			// Find all islands

			var remaining = _sIslandSet ??= new HashSet<CsgIsland>();
			remaining.Clear();

			foreach ( var (_, cell) in _grid )
			{
				foreach ( var island in cell.Islands )
				{
					remaining.Add( island );
				}
			}

			// Find chunks of connected islands

			var queue = _sIslandQueue ??= new Queue<CsgIsland>();
			var chunks = _sChunks ??= new List<(CsgIsland Root, int Count, float Volume)>();

			chunks.Clear();

			while ( remaining.Count > 0 )
			{
				queue.Clear();

				var root = remaining.First();
				var volume = root.Volume;
				var count = 1;

				remaining.Remove( root );
				queue.Enqueue( root );

				while ( queue.TryDequeue( out var next ) )
				{
					foreach ( var neighbor in next.Neighbors )
					{
						if ( remaining.Remove( neighbor ) )
						{
							queue.Enqueue( neighbor );

							volume += neighbor.Volume;
							count += 1;
						}
					}
				}

				chunks.Add( (root, count, volume) );
			}

			// Sort by volume (descending)

			chunks.Sort( ( a, b ) => Math.Sign( b.Volume - a.Volume ) );

			if ( LogTimings )
			{
				Log.Info( $"Chunks: {chunks.Count}" );

				foreach ( var chunk in chunks )
				{
					Log.Info( $"  {chunk.Volume}, {chunk.Count}" );
				}
			}

			// Handle the whole solid being too small / empty

			if ( chunks.Count == 0 || chunks[0].Volume < MinVolume )
			{
				Deleted = true;

				if ( IsClientOnly || Game.IsServer )
				{
					Delete();
				}
				else
				{
					EnableDrawing = false;
					PhysicsEnabled = false;
					EnableSolidCollisions = false;

					DeleteSceneObjects();
				}

				return true;
			}

			if ( chunks.Count == 1 ) return false;

			// Time to split, let's wake up

			if ( !IsStatic && PhysicsBody != null )
			{
				PhysicsBody.Sleeping = false;
			}

			// Leave most voluminous chunk in this solid, but create new solids for the rest

			var visited = remaining;

			foreach ( var chunk in chunks.Skip( 1 ) )
			{
				visited.Clear();
				queue.Clear();

				queue.Enqueue( chunk.Root );
				visited.Add( chunk.Root );

				while ( queue.Count > 0 )
				{
					var next = queue.Dequeue();

					foreach ( var neighbor in next.Neighbors )
					{
						if ( visited.Add( neighbor ) )
						{
							queue.Enqueue( neighbor );
						}
					}
				}

				var child = chunk.Volume < MinVolume ? null : new CsgSolid( 0f )
				{
					IsStatic = false,
					PhysicsEnabled = true,
					Transform = Transform
				};

				foreach ( var island in visited )
				{
					foreach ( var hull in island.Hulls )
					{
						RemoveHull( hull );
						child?.AddHull( hull );
					}
				}

				if ( child == null )
				{
					continue;
				}

				var disconnectionIndex = _nextDisconnectionIndex++;

				if ( Game.IsServer )
				{
					child.ServerDisconnectionIndex = disconnectionIndex;
					child.ServerDisconnectedFrom = this;
				}

				if ( Game.IsClient )
				{
					child.ClientDisconnectionIndex = disconnectionIndex;

					ClientDisconnections.Add( disconnectionIndex, child );
				}
			}

			return true;
		}
	}
}
