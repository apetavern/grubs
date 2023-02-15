namespace Sandbox.Csg
{
	[Category( "Terrain" )]
	public partial class CsgSolid : ModelEntity
	{
		[ConVar.Server( "csg_log", Help = "If set, CSG timing info is logged" )]
		public static bool LogTimings { get; set; }

		public CsgSolid()
		{
			Game.AssertClient( nameof( CsgSolid ) );

			Tags.Add( "solid" );
		}

		public CsgSolid( Vector3 gridSize )
		{
			GridSize = gridSize;

			SetupContainers( GridSize );

			Tags.Add( "solid" );
		}

		public override void Spawn()
		{
			base.Spawn();

			Transmit = TransmitType.Always;

			Tags.Add( "solid" );
		}

		public override void ClientSpawn()
		{
			base.ClientSpawn();

			SetupContainers( GridSize );

			Tags.Add( "solid" );
		}

		[Event.Tick.Server]
		private void ServerTick()
		{
			if ( _invalidConnectivity.Count > 0 )
			{
				if ( Disconnect() && Deleted )
				{
					return;
				}
			}

			SendModifications();
			CollisionUpdate();
		}

		[Event.Tick.Client]
		private void ClientTick()
		{
			if ( !IsClientOnly )
			{
				CheckInitialGeometry();
			}

			if ( Deleted ) return;

			MeshUpdate();
			CollisionUpdate();
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();

			DeleteSceneObjects();
		}

		private void DeleteSceneObjects()
		{
			foreach ( var (_, cell) in _grid )
			{
				cell.SceneObject?.Delete();
				cell.SceneObject = null;
			}
		}

		private void Clear( bool removeColliders )
		{
			if ( removeColliders )
			{
				foreach ( var pair in _grid )
				{
					foreach ( var hull in pair.Value.Hulls )
					{
						hull.RemoveCollider();

						hull.Island = null;
						hull.GridCell = null;
						hull.GridCoord = default;
					}
				}
			}

			_grid.Clear();
		}

		public void AddHull( CsgHull hull )
		{
			if ( hull.IsEmpty )
			{
				throw new ArgumentException( "Can't add an empty hull", nameof( hull ) );
			}

			if ( !HasGrid )
			{
				AddHull( hull, default );
				return;
			}

			var bounds = hull.VertexBounds;
			var minCoord = GetGridCoord( bounds.Mins );
			var maxCoord = GetGridCoord( bounds.Maxs );

			if ( minCoord == maxCoord )
			{
				AddHull( hull, minCoord );
				return;
			}

			var toAdd = CsgHelpers.RentHullList();

			try
			{
				toAdd.Add( hull );

				SubdivideGridAxis( new Vector3( 1f, 0f, 0f ), toAdd );
				SubdivideGridAxis( new Vector3( 0f, 1f, 0f ), toAdd );
				SubdivideGridAxis( new Vector3( 0f, 0f, 1f ), toAdd );

				foreach ( var subHull in toAdd )
				{
					AddHull( subHull, GetGridCoord( subHull.VertexBounds.Center ) );
				}
			}
			finally
			{
				CsgHelpers.Return( toAdd );
			}
		}

		private void AddHull( CsgHull hull, (int X, int Y, int Z) gridCoord )
		{
			if ( hull.GridCell != null )
			{
				throw new Exception( "Hull has already been added to a cell" );
			}

			if ( !_grid.TryGetValue( gridCoord, out var cell ) )
			{
				_grid[gridCoord] = cell = new GridCell { Solid = this };
			}

			hull.GridCoord = gridCoord;
			hull.GridCell = cell;
			hull.Island = null;

			cell.Hulls.Add( hull );

			cell.InvalidateCollision();
			cell.InvalidateMesh();
			cell.InvalidateConnectivity();
		}

		private void RemoveHull( CsgHull hull )
		{
			if ( hull.GridCell?.Solid != this )
			{
				throw new Exception( "Hull isn't owned by this solid" );
			}

			var cell = hull.GridCell;

			Assert.True( cell.Hulls.Remove( hull ) );

			hull.RemoveCollider();

			hull.GridCell = null;
			hull.GridCoord = default;
			hull.Island = null;

			cell.InvalidateCollision();
			cell.InvalidateMesh();
			cell.InvalidateConnectivity();
		}
	}
}
