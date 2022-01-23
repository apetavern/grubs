using System;
using Sandbox;
using System.Collections.Generic;
using System.Linq;
using Grubs.Pawn;
using System.Runtime.InteropServices;

namespace Grubs.Weapons
{
	public struct ArcSegment
	{
		public Vector3 StartPos { get; set; }
		public Vector3 EndPos { get; set; }

		public Vector3 HitNormal { get; set; }
	}

	public class ArcTrace
	{
		public Vector3 StartPos { get; set; }
		public Vector3 EndPos { get; set; }
		private int SegmentCount { get; set; } = 80;
		private Entity Owner { get; set; }

		public ArcTrace( Entity fromGrub, Vector3 startPos )
		{
			StartPos = startPos;
			Owner = fromGrub;
		}

		/// <summary>
		/// Run a bezier trace with a specific pre-determined end position.
		/// </summary>
		public List<ArcSegment> RunTo( Vector3 EndPos )
		{
			var segments = new List<ArcSegment>();
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

				var tr = Trace.Ray( segment.StartPos, segment.EndPos ).Ignore( Owner ).Radius( 2f ).Run();

				if ( tr.Hit )
				{
					EndPos = tr.EndPos;

					segment.EndPos = EndPos;
					segments.Add( segment );

					break;
				}

				segments.Add( segment );
			}

			return segments;
		}

		public List<ArcSegment> RunTowards( Vector3 direction, float force, float windForceX )
		{
			return RunTowards( StartPos, direction, force, windForceX );
		}

		/// <summary>
		/// Run a trace specifying the origin, direction, force and wind force.
		/// </summary>
		public List<ArcSegment> RunTowards( Vector3 startPos, Vector3 direction, float force, float windForceX )
		{
			var segments = new List<ArcSegment>();
			float epsilon = 0.001f;

			Vector3 velocity = force * -direction;
			Vector3 position = startPos;

			for ( int i = 0; i < SegmentCount; i++ )
			{
				ArcSegment segment = new();
				segment.StartPos = position;

				velocity -= new Vector3( windForceX, 0, 0 );
				velocity -= PhysicsWorld.Gravity * epsilon;
				position -= velocity;

				segment.EndPos = position;

				var tr = Trace.Ray( segment.StartPos, segment.EndPos ).Ignore( Owner ).Radius( 2f ).Run();

				if ( tr.Hit )
				{
					var travelDir = (tr.StartPos - tr.EndPos).Normal;
					segment.EndPos = tr.EndPos + travelDir;
					segment.HitNormal = tr.Normal;
					segments.Add( segment );

					return segments;
				}

				segments.Add( segment );
			}

			return segments;
		}

		public List<ArcSegment> RunTowardsWithBounces( Vector3 direction, float force, float windForceX, int bounceQty )
		{
			List<ArcSegment> segments = new();

			var trace = RunTowards( StartPos, direction, force, windForceX );
			var activeForce = force;

			for ( int i = 0; i < bounceQty; i++ )
			{
				segments.AddRange( trace );

				var traceEnd = trace.Last();
				activeForce *= 0.66f;

				DebugOverlay.Line( traceEnd.EndPos, traceEnd.EndPos + traceEnd.HitNormal * 10, Color.Red, 0, true );

				trace = RunTowards( traceEnd.EndPos, traceEnd.HitNormal, activeForce, windForceX );
			}

			return segments;
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
