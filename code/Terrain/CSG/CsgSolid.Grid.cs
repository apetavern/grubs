using System;
using System.Collections.Generic;

namespace Sandbox.Csg
{
	partial class CsgSolid
	{
		[Net]
		public Vector3 GridSize { get; set; }

		private Vector3 _gridSize;
		private Vector3 _invGridSize;

		public bool HasGrid { get; private set; }

		private readonly HashSet<GridCell> _invalidCollision = new();
		private readonly HashSet<GridCell> _invalidMesh = new();
		private readonly HashSet<GridCell> _invalidConnectivity = new();

		internal class GridCell
		{
			public List<CsgHull> Hulls { get; } = new List<CsgHull>();
			public List<CsgIsland> Islands { get; } = new List<CsgIsland>();

			public Dictionary<int, Mesh> Meshes { get; } = new();

			public SceneObject SceneObject { get; set; }

			private CsgSolid _solid;

			public CsgSolid Solid
			{
				get => _solid;
				set
				{
					if ( _solid == value ) return;

					_solid = value;

					_solid?._invalidCollision.Add( this );
					_solid?._invalidMesh.Add( this );
					_solid?._invalidConnectivity.Add( this );
				}
			}

			public float Volume { get; set; }
			public float Mass { get; set; }

			public bool CollisionInvalid { get; private set; }
			public bool MeshInvalid { get; private set; }
			public bool ConnectivityInvalid { get; private set; }

			public void InvalidateCollision()
			{
				if ( CollisionInvalid ) return;

				CollisionInvalid = true;

				Solid?._invalidCollision.Add( this );
			}

			public void PostCollisionUpdate()
			{
				CollisionInvalid = false;
			}

			public void InvalidateMesh()
			{
				if ( MeshInvalid ) return;

				MeshInvalid = true;

				Solid?._invalidMesh.Add( this );
			}

			public void PostMeshUpdate()
			{
				MeshInvalid = false;
			}

			public void InvalidateConnectivity()
			{
				if ( ConnectivityInvalid ) return;

				ConnectivityInvalid = true;

				Solid?._invalidConnectivity.Add( this );
			}

			public void PostConnectivityUpdate()
			{
				ConnectivityInvalid = false;
			}

			public CsgIsland GetOrCreateIsland( int index )
			{
				if ( index < Islands.Count ) return Islands[index];

				CsgHelpers.AssertAreEqual( Islands.Count, index );

				var island = new CsgIsland();

				Islands.Add( island );

				return island;
			}
		}

		private Dictionary<(int X, int Y, int Z), GridCell> _grid;

		private void SetupContainers( Vector3 gridSize )
		{
			if ( _grid != null )
			{
				throw new Exception( "Containers already set up" );
			}

			if ( LogTimings )
			{
				Log.Info( $"SetupContainers( {gridSize} )" );
			}

			if ( gridSize.x < 0f || gridSize.y < 0f || gridSize.z < 0f )
			{
				throw new ArgumentException( "Grid size must be non-negative.", nameof( gridSize ) );
			}

			_gridSize = gridSize;

			_invGridSize.x = gridSize.x <= 0f ? 0f : 1f / gridSize.x;
			_invGridSize.y = gridSize.y <= 0f ? 0f : 1f / gridSize.y;
			_invGridSize.z = gridSize.z <= 0f ? 0f : 1f / gridSize.z;

			HasGrid = _gridSize != Vector3.Zero;

			_grid = new Dictionary<(int X, int Y, int Z), GridCell>();

			if ( Game.IsClient )
			{
				OnModificationsChanged();
			}
		}

		private void SubdivideGridAxis( Vector3 axis, List<CsgHull> hulls )
		{
			var gridSize = Vector3.Dot( axis, _gridSize );

			if ( gridSize <= 0f ) return;

			for ( var i = hulls.Count - 1; i >= 0; i-- )
			{
				var poly = hulls[i];
				var bounds = poly.VertexBounds;

				var min = Vector3.Dot( bounds.Mins, axis );
				var max = Vector3.Dot( bounds.Maxs, axis );

				var minGrid = (int)MathF.Floor( min / gridSize ) + 1;
				var maxGrid = (int)MathF.Ceiling( max / gridSize ) - 1;

				for ( var grid = minGrid; grid <= maxGrid; grid++ )
				{
					var plane = new CsgPlane( axis, grid * gridSize );
					var child = poly.Split( plane );

					if ( child != null )
					{
						hulls.Add( child );
					}
				}
			}
		}

		private (int X, int Y, int Z) GetGridCoord( Vector3 pos )
		{
			return HasGrid
				? (
					(int)MathF.Floor( pos.x * _invGridSize.x ),
					(int)MathF.Floor( pos.y * _invGridSize.y ),
					(int)MathF.Floor( pos.z * _invGridSize.z ))
				: default;
		}

		private int GetAllHulls( List<CsgHull> outHulls )
		{
			var count = 0;

			foreach ( var pair in _grid )
			{
				outHulls.AddRange( pair.Value.Hulls );
				count += pair.Value.Hulls.Count;
			}

			return count;
		}

		public int GetHullsTouching( CsgHull hull, List<CsgHull> outHulls )
		{
			// First pass: cheap BBox test

			var count = GetHullsTouching( hull.VertexBounds, outHulls );

			CsgHelpers.AssertAreEqual( count, outHulls.Count );

			// Second pass: actual intersection check

			for ( var i = count - 1; i >= 0; i-- )
			{
				if ( hull.IsTouching( outHulls[i] ) )
				{
					continue;
				}

				count--;

				(outHulls[count], outHulls[i]) = (outHulls[i], outHulls[count]);
			}

			if ( LogTimings )
			{
				Log.Info( $"Before: {outHulls.Count}, after: {count}" );
			}

			outHulls.RemoveRange( count, outHulls.Count - count );

			return count;
		}

		private int GetHullsTouching( BBox bounds, List<CsgHull> outHulls )
		{
			Assert.False( bounds.Mins.IsNaN );
			Assert.False( bounds.Maxs.IsNaN );

			Assert.False( float.IsInfinity( bounds.Mins.x ) );
			Assert.False( float.IsInfinity( bounds.Mins.y ) );
			Assert.False( float.IsInfinity( bounds.Mins.z ) );

			Assert.False( float.IsInfinity( bounds.Maxs.x ) );
			Assert.False( float.IsInfinity( bounds.Maxs.y ) );
			Assert.False( float.IsInfinity( bounds.Maxs.z ) );

			bounds.Mins -= CsgHelpers.DistanceEpsilon;
			bounds.Maxs += CsgHelpers.DistanceEpsilon;

			var insideCount = 0;

			var gridMin = GetGridCoord( bounds.Mins );
			var gridMax = GetGridCoord( bounds.Maxs );

			var cellCount = (gridMax.X - gridMin.X + 1) * (gridMax.Y - gridMin.Y + 1) * (gridMax.Z - gridMin.Z + 1);

			if ( cellCount > _grid.Count )
			{
				foreach ( var pair in _grid )
				{
					if ( pair.Key.X < gridMin.X || pair.Key.Y < gridMin.Y || pair.Key.Z < gridMin.Z ) continue;
					if ( pair.Key.X > gridMax.X || pair.Key.Y > gridMax.Y || pair.Key.Z > gridMax.Z ) continue;

					foreach ( var hull in pair.Value.Hulls )
					{
						if ( hull.IsEmpty ) continue;
						if ( !hull.VertexBounds.Overlaps( bounds ) ) continue;

						outHulls.Add( hull );
						++insideCount;
					}
				}

				return insideCount;
			}

			for ( var x = gridMin.X; x <= gridMax.X; x++ )
			{
				for ( var y = gridMin.Y; y <= gridMax.Y; y++ )
				{
					for ( var z = gridMin.Z; z <= gridMax.Z; z++ )
					{
						if ( !_grid.TryGetValue( (x, y, z), out var cell ) ) continue;

						foreach ( var hull in cell.Hulls )
						{
							if ( hull.IsEmpty ) continue;
							if ( !hull.VertexBounds.Overlaps( bounds ) ) continue;

							outHulls.Add( hull );
							++insideCount;
						}
					}
				}
			}

			return insideCount;
		}
	}
}
