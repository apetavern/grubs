using Sandbox;
using System.Collections.Generic;

namespace TerryForm.Weapons
{
	public struct ArcSegment
	{
		public Vector3 StartPos { get; set; }
		public Vector3 EndPos { get; set; }
	}

	public class ArcTrace
	{
		public Vector3 StartPos { get; set; }
		public Vector3 Direction { get; set; }
		public Vector3 EndPos { get; set; }
		public Vector3 WindForce { get; set; }
		private float Force { get; set; }
		private int SegmentCount { get; set; } = 80;

		public List<ArcSegment> Segments { get; set; } = new();

		public ArcTrace( Vector3 startPos, Vector3 direction, float force, Vector3 wind )
		{
			StartPos = startPos;
			Direction = direction;
			Force = force;
			WindForce = wind;
		}

		public List<ArcSegment> Run()
		{
			float epsilon = 0.001f;

			Vector3 velocity = Force * -Direction;
			Vector3 position = StartPos;

			for ( int i = 0; i < SegmentCount; i++ )
			{
				ArcSegment segment = new();
				segment.StartPos = position;

				velocity -= WindForce;
				velocity -= PhysicsWorld.Gravity * epsilon;
				position -= velocity;

				segment.EndPos = position;

				var tr = Trace.Ray( segment.StartPos, segment.EndPos ).Radius( 2f ).WorldOnly().Run();

				if ( tr.Hit )
				{
					EndPos = tr.EndPos;
					break;
				}

				Segments.Add( segment );
			}

			DrawSegments();

			return Segments;
		}

		public void DrawSegments()
		{
			foreach ( var segment in Segments )
			{
				DebugOverlay.Line( segment.StartPos, segment.EndPos );
			}
		}
	}
}
