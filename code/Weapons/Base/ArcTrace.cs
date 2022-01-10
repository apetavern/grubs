using System;
using Sandbox;
using System.Collections.Generic;
using System.Linq;
using Grubs.Pawn;

namespace Grubs.Weapons
{
	public struct ArcSegment
	{
		public Vector3 StartPos { get; set; }
		public Vector3 EndPos { get; set; }
	}

	public class ArcTrace
	{
		public Vector3 StartPos { get; set; }
		public Vector3 EndPos { get; set; }
		private int SegmentCount { get; set; } = 80;
		public List<ArcSegment> Segments { get; set; } = new();

		public ArcTrace( Vector3 startPos )
		{
			StartPos = startPos;
		}

		/// <summary>
		/// Run a bezier trace with a specific pre-determined end position.
		/// </summary>
		public List<ArcSegment> RunTo( Vector3 EndPos )
		{
			var from = StartPos;
			var controlPoint = EndPos.WithZ( from.z * 3 );

			for ( int i = 1; i <= SegmentCount; i++ )
			{
				var offset = (float)i / (SegmentCount / 2);
				var position = (float)Math.Pow( 1 - offset, 3 ) * StartPos + 1 * (1 - offset) * offset * controlPoint + (float)Math.Pow( offset, 3 ) * EndPos;

				ArcSegment segment = new();
				segment.StartPos = from;
				from = position;
				segment.EndPos = from;

				var tr = Trace.Ray( segment.StartPos, segment.EndPos ).Radius( 2f ).Run();

				if ( tr.Hit )
				{
					EndPos = tr.EndPos;

					segment.EndPos = EndPos;
					Segments.Add( segment );

					break;
				}

				Segments.Add( segment );
			}

			return Segments;
		}

		/// <summary>
		/// Run a trace specifying the direction, force and wind force.
		/// </summary>
		public List<ArcSegment> RunTowards( Worm fromWorm, Vector3 direction, float force, float windForceX )
		{
			float epsilon = 0.001f;

			Vector3 velocity = force * -direction;
			Vector3 position = StartPos;

			for ( int i = 0; i < SegmentCount; i++ )
			{
				ArcSegment segment = new();
				segment.StartPos = position;

				velocity -= new Vector3( windForceX, 0, 0 );
				velocity -= PhysicsWorld.Gravity * epsilon;
				position -= velocity;

				segment.EndPos = position;

				var tr = Trace.Ray( segment.StartPos, segment.EndPos ).Ignore( fromWorm ).Radius( 2f ).Run();

				if ( tr.Hit )
				{
					EndPos = tr.EndPos;

					segment.EndPos = EndPos;
					Segments.Add( segment );

					break;
				}

				Segments.Add( segment );
			}

			return Segments;
		}

		public static void Draw( List<ArcSegment> segments )
		{
			foreach ( var segment in segments )
			{
				DebugOverlay.Line( segment.StartPos, segment.EndPos );
			}
		}
	}
}
