namespace Sandbox.Csg
{
	public enum CsgOperator
	{
		Add,
		Subtract,
		Replace,
		Paint,
		Disconnect
	}

	internal record struct CsgModification( CsgOperator Operator, CsgBrush Brush, CsgMaterial Material, Matrix? Transform );

	partial class CsgSolid
	{
		private const int MaxModificationsPerMessage = 8;
		private const int SendModificationsRpc = 269924031; // CsgSolid.SendModifications

		private int _appliedModifications;
		private readonly List<CsgModification> _modifications = new List<CsgModification>();
		private readonly Dictionary<IClient, int> _sentModifications = new Dictionary<IClient, int>();

		public TimeSince TimeSinceLastModification { get; private set; }

		[ThreadStatic]
		private static List<IClient> _sToRemove;

		private void AddModification( in CsgModification modification )
		{
			_modifications.Add( modification );
		}

		private void SendModifications()
		{
			_sToRemove ??= new List<IClient>();
			_sToRemove.Clear();

			foreach ( var (pawn, _) in _sentModifications )
			{
				if ( !pawn.IsValid )
				{
					_sToRemove.Add( pawn );
				}
			}

			foreach ( var client in _sToRemove )
			{
				_sentModifications.Remove( client );
			}

			foreach ( var client in Game.Clients )
			{
				if ( client.IsBot ) continue;

				if ( !_sentModifications.TryGetValue( client, out var prevCount ) )
				{
					prevCount = 0;
					_sentModifications.Add( client, prevCount );
				}

				Assert.True( prevCount <= _modifications.Count );

				if ( prevCount == _modifications.Count ) continue;

				var msg = NetWrite.StartRpc( SendModificationsRpc, this );

				var msgCount = Math.Min( _modifications.Count - prevCount, MaxModificationsPerMessage );

				msg.Write( prevCount );
				msg.Write( msgCount );
				msg.Write( _modifications.Count );

				for ( var i = 0; i < msgCount; i++ )
				{
					WriteModification( msg, _modifications[prevCount + i] );
				}

				msg.SendRpc( To.Single( client ), null );

				TimeSinceLastModification = 0;

				_sentModifications[client] = prevCount + msgCount;
			}
		}

		private void ReceiveModifications( ref NetRead read )
		{
			var prevCount = read.Read<int>();
			var msgCount = read.Read<int>();
			var totalCount = read.Read<int>();

			CsgHelpers.AssertAreEqual( prevCount, _modifications.Count );

			for ( var i = 0; i < msgCount; ++i )
			{
				_modifications.Add( ReadModification( ref read ) );
			}

			OnModificationsChanged();
		}

		private static void WriteModification( NetWrite writer, in CsgModification value )
		{
			// Write operator

			writer.Write( value.Operator );

			// Write brush

			switch ( value.Operator )
			{
				case CsgOperator.Disconnect:
					break;

				default:
					Assert.NotNull( value.Brush );
					value.Brush.Serialize( writer );
					break;
			}

			// Write material

			switch ( value.Operator )
			{
				case CsgOperator.Disconnect:
				case CsgOperator.Subtract:
					break;

				default:
					Assert.NotNull( value.Material );
					value.Material.Serialize( writer );
					break;
			}

			// Write transform

			switch ( value.Operator )
			{
				case CsgOperator.Disconnect:
					break;

				default:
					writer.Write( value.Transform.HasValue );

					if ( value.Transform.HasValue )
					{
						writer.Write( value.Transform.Value );
					}
					break;
			}
		}

		private static CsgModification ReadModification( ref NetRead reader )
		{
			CsgBrush brush = null;
			CsgMaterial material = null;
			Matrix? transform = null;

			// Read operator

			var op = reader.Read<CsgOperator>();

			// Read brush

			switch ( op )
			{
				case CsgOperator.Disconnect:
					break;

				default:
					brush = CsgBrush.Deserialize( ref reader );
					break;
			}

			// Read material

			switch ( op )
			{
				case CsgOperator.Disconnect:
				case CsgOperator.Subtract:
					break;

				default:
					material = CsgMaterial.Deserialize( ref reader );
					break;
			}

			// Read transform

			switch ( op )
			{
				case CsgOperator.Disconnect:
					break;

				default:
					if ( reader.Read<bool>() )
					{
						transform = reader.Read<Matrix>();
					}
					break;
			}

			return new CsgModification( op, brush, material, transform );
		}

		protected override void OnCallRemoteProcedure( int id, NetRead read )
		{
			switch ( id )
			{
				case SendModificationsRpc:
					ReceiveModifications( ref read );
					break;

				default:
					base.OnCallRemoteProcedure( id, read );
					break;
			}
		}

		private static Matrix? CreateMatrix( Vector3? position = null, Vector3? scale = null, Rotation? rotation = null )
		{
			if ( position == null && scale == null && rotation == null )
			{
				return null;
			}

			var transform = Matrix.Identity;

			if ( position != null )
			{
				transform = Matrix.CreateTranslation( position.Value );
			}

			if ( scale != null )
			{
				transform = Matrix.CreateScale( scale.Value ) * transform;
			}

			if ( rotation != null )
			{
				transform = Matrix.CreateRotation( rotation.Value ) * transform;
			}

			return transform;
		}

		public bool Add( CsgBrush brush, CsgMaterial material,
			Vector3? position = null, Vector3? scale = null, Rotation? rotation = null )
		{
			Assert.NotNull( material );

			return Modify( CsgOperator.Add, brush, material, CreateMatrix( position, scale, rotation ) );
		}

		public bool Subtract( CsgBrush brush,
			Vector3? position = null, Vector3? scale = null, Rotation? rotation = null )
		{
			return Modify( CsgOperator.Subtract, brush, null, CreateMatrix( position, scale, rotation ) );
		}

		public bool Replace( CsgBrush brush, CsgMaterial material,
			Vector3? position = null, Vector3? scale = null, Rotation? rotation = null )
		{
			Assert.NotNull( material );

			return Modify( CsgOperator.Replace, brush, material, CreateMatrix( position, scale, rotation ) );
		}

		public bool Paint( CsgBrush brush, CsgMaterial material,
			Vector3? position = null, Vector3? scale = null, Rotation? rotation = null )
		{
			Assert.NotNull( material );

			return Modify( CsgOperator.Paint, brush, material, CreateMatrix( position, scale, rotation ) );
		}

		private bool Disconnect()
		{
			return Modify( CsgOperator.Disconnect, null, null, default );
		}

		private void OnModificationsChanged()
		{
			if ( _grid == null ) return;

			if ( ServerDisconnectedFrom != null && !_copiedInitialGeometry )
			{
				return;
			}

			while ( _appliedModifications < _modifications.Count )
			{
				ModifyInternal( _modifications[_appliedModifications++] );
			}
		}

		private Matrix WorldToLocal => Matrix.CreateTranslation( -Position )
			* Matrix.CreateScale( 1f / Scale )
			* Matrix.CreateRotation( Rotation.Inverse );

		private bool Modify( CsgOperator op, CsgBrush brush, CsgMaterial material, in Matrix? transform )
		{
			Game.AssertServer( nameof( Modify ) );

			var mod = new CsgModification( op, brush, material, transform.HasValue ? transform.Value * WorldToLocal : null );

			if ( ModifyInternal( mod ) )
			{
				AddModification( mod );
				return true;
			}

			return false;
		}

		private bool ModifyInternal( in CsgModification modification )
		{
			if ( Deleted )
			{
				if ( Game.IsServer )
				{
					Log.Warning( $"Attempting to modify a deleted {nameof( CsgSolid )}" );
				}

				return false;
			}

			Timer.Restart();

			var hulls = CsgHelpers.RentHullList();

			try
			{
				if ( modification.Operator == CsgOperator.Disconnect )
				{
					return ConnectivityUpdate();
				}

				modification.Brush.CreateHulls( hulls );

				var changed = false;

				foreach ( var solid in hulls )
				{
					solid.Material = modification.Material;

					if ( modification.Transform.HasValue )
					{
						solid.Transform( modification.Transform.Value );
					}
				}

				foreach ( var solid in hulls )
				{
					changed |= Modify( solid, modification.Operator );
				}

				return changed;
			}
			finally
			{
				CsgHelpers.Return( hulls );

				if ( LogTimings )
				{
					Log.Info( $"Modify {modification.Operator}: {Timer.Elapsed.TotalMilliseconds:F2}ms" );
				}
			}
		}

		private bool Modify( CsgHull solid, CsgOperator op )
		{
			if ( solid.IsEmpty ) return false;

			var faces = solid.Faces;

			var nearbyHulls = CsgHelpers.RentHullList();
			var addedHulls = CsgHelpers.RentHullList();
			var removedHulls = CsgHelpers.RentHullList();
			var changedHulls = CsgHelpers.RentHullSet();

			var changed = false;

			try
			{
				var elapsedBefore = Timer.Elapsed;

				GetHullsTouching( solid, nearbyHulls );

				if ( LogTimings )
				{
					Log.Info( $"GetHullsTouching: {(Timer.Elapsed - elapsedBefore).TotalMilliseconds:F2}ms" );
				}

				foreach ( var next in nearbyHulls )
				{
					var skip = false;
					var nextChanged = false;

					switch ( op )
					{
						case CsgOperator.Replace:
							changed |= nextChanged = next.Paint( solid, null );
							skip = next.Material == solid.Material;
							break;

						case CsgOperator.Paint:
							changed |= nextChanged = next.Paint( solid, solid.Material );
							skip = true;
							break;

						case CsgOperator.Add:
							for ( var faceIndex = 0; faceIndex < faces.Count; ++faceIndex )
							{
								var solidFace = faces[faceIndex];

								if ( !next.TryGetFace( -solidFace.Plane, out var nextFace ) )
								{
									continue;
								}

								skip = true;

								if ( ConnectFaces( solidFace, solid, nextFace, next ) )
								{
									changed = nextChanged = true;
								}
								break;
							}
							break;
					}

					if ( skip )
					{
						if ( nextChanged )
						{
							changedHulls.Add( next );
						}

						continue;
					}

					for ( var faceIndex = 0; faceIndex < faces.Count && !next.IsEmpty; ++faceIndex )
					{
						var face = faces[faceIndex];
						var child = next.Split( face.Plane, face.FaceCuts );

						if ( child == null )
						{
							continue;
						}

						changed = nextChanged = true;

						if ( child.Faces.Count < 4 )
						{
							child.SetEmpty( null );
						}
						else if ( !child.IsEmpty )
						{
							addedHulls.Add( child );
							changedHulls.Add( child );
						}

						if ( next.Faces.Count < 4 )
						{
							next.SetEmpty( null );
						}
					}

					if ( next.IsEmpty )
					{
						changed = true;

						removedHulls.Add( next );
						continue;
					}

					if ( solid.GetSign( next.VertexAverage ) < 0 )
					{
						if ( nextChanged )
						{
							changedHulls.Add( next );
						}

						continue;
					}

					// next will now contain only the intersection with solid.
					// We'll copy its faces and remove it

					switch ( op )
					{
						case CsgOperator.Replace:
							changed = true;

							next.Material = solid.Material;
							next.InvalidateMesh();

							changedHulls.Add( next );
							break;

						case CsgOperator.Add:
							changed = true;

							removedHulls.Add( next );

							solid.MergeSubFacesFrom( next );
							next.SetEmpty( null );
							break;

						case CsgOperator.Subtract:
							changed = true;

							removedHulls.Add( next );

							next.SetEmpty( null );
							break;
					}
				}

				switch ( op )
				{
					case CsgOperator.Add:
						changed = true;

						solid.RemoveCollider();

						addedHulls.Add( solid );
						changedHulls.Add( solid );
						break;
				}

				foreach ( var hull in removedHulls )
				{
					RemoveHull( hull );
				}

				foreach ( var hull in addedHulls )
				{
					AddHull( hull );
				}

				// Try to merge adjacent hulls / sub faces

				var hullMergeCount = 0;
				var subFaceMergeCount = 0;

				var elapsed = Timer.Elapsed;

				foreach ( var hull in changedHulls )
				{
					if ( hull.IsEmpty ) continue;

					if ( op != CsgOperator.Paint )
					{
						bool merged;

						do
						{
							merged = false;

							nearbyHulls.Clear();

							hull.GetNeighbors( nearbyHulls );

							foreach ( var neighbor in nearbyHulls )
							{
								Assert.NotNull( neighbor.GridCell );

								if ( hull.TryMerge( neighbor ) )
								{
									++hullMergeCount;

									RemoveHull( neighbor );

									merged = true;
									break;
								}
							}
						} while ( merged );
					}

					subFaceMergeCount += hull.MergeSubFaces();
				}

				if ( hullMergeCount + subFaceMergeCount > 0 && LogTimings )
				{
					Log.Info( $"Merged {hullMergeCount} hulls, {subFaceMergeCount} sub faces in {(Timer.Elapsed - elapsed).TotalMilliseconds:F2}ms" );
				}
			}
			finally
			{
				CsgHelpers.Return( nearbyHulls );
				CsgHelpers.Return( addedHulls );
				CsgHelpers.Return( removedHulls );
				CsgHelpers.Return( changedHulls );
			}

			return changed;
		}

		private static bool ConnectFaces( CsgHull.Face faceA, CsgHull solidA, CsgHull.Face faceB, CsgHull solidB )
		{
			var intersectionCuts = CsgHelpers.RentFaceCutList();

			var faceAHelper = faceA.Plane.GetHelper();
			var faceBHelper = faceB.Plane.GetHelper();

			try
			{
				intersectionCuts.AddRange( faceA.FaceCuts );

				foreach ( var faceCut in faceB.FaceCuts )
				{
					intersectionCuts.Split( -faceBHelper.Transform( faceCut, faceAHelper ) );
				}

				if ( intersectionCuts.IsDegenerate() || solidB.GetSign( faceAHelper.GetAveragePos( intersectionCuts ) ) < 0 )
				{
					return false;
				}

				faceA.RemoveSubFacesInside( intersectionCuts );
				faceA.SubFaces.Add( new CsgHull.SubFace
				{
					FaceCuts = new List<CsgHull.FaceCut>( intersectionCuts ),
					Neighbor = solidB
				} );

				for ( var i = 0; i < intersectionCuts.Count; i++ )
				{
					intersectionCuts[i] = -faceAHelper.Transform( intersectionCuts[i], faceBHelper );
				}

				faceB.RemoveSubFacesInside( intersectionCuts );
				faceB.SubFaces.Add( new CsgHull.SubFace
				{
					FaceCuts = new List<CsgHull.FaceCut>( intersectionCuts ),
					Neighbor = solidA
				} );

				solidA.InvalidateNeighbors();
				solidB.InvalidateNeighbors();

				solidA.InvalidateMesh();
				solidB.InvalidateMesh();

				return true;
			}
			finally
			{
				CsgHelpers.Return( intersectionCuts );
			}
		}
	}
}
