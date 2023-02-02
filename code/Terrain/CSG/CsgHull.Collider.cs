using System.IO;
using System.Text;

namespace Sandbox.Csg
{
	partial class CsgHull
	{
		[ConVar.Replicated( "csg_write_last_hull" )]
		private bool WriteLastHullToFile { get; set; } = false;

		public PhysicsShape Collider { get; internal set; }

		public void InvalidateCollision()
		{
			GridCell?.InvalidateCollision();
			GridCell?.InvalidateConnectivity();

			RemoveCollider();
		}

		public void RemoveCollider()
		{
			if ( Collider.IsValid() && Collider.Body.IsValid() )
			{
				Collider.Remove();
			}

			Collider = null;
		}

		public bool UpdateCollider( PhysicsBody body )
		{
			if ( Collider.IsValid() ) return false;
			if ( IsEmpty ) return false;

			RemoveCollider();

			UpdateVertices();

			Assert.True( _vertices.Count > 3 );

			if ( WriteLastHullToFile )
			{
				var writer = new StringBuilder();

				foreach ( var vertex in _vertices )
				{
					writer.AppendLine( $"{vertex.x:R}, {vertex.y:R}, {vertex.z:R}" );
				}

				FileSystem.Data.WriteAllText( "last-hull.txt", writer.ToString() );
			}

			Collider = body.AddHullShape( Vector3.Zero, Rotation.Identity, _vertices );

			return true;
		}
	}
}
