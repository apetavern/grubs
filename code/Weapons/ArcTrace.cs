namespace Grubs;

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
	private Grub Owner { get; set; }
	private Gadget Gadget { get; set; }

	public ArcTrace( Grub fromGrub, Gadget gadget, Vector3 startPos )
	{
		StartPos = startPos;
		Owner = fromGrub;
		Gadget = gadget;
	}

	/// <summary>
	/// Run a bezier trace with a specific pre-determined end position.
	/// </summary>
	public List<ArcSegment> RunTo( Vector3 endPos )
	{
		var segments = new List<ArcSegment>();
		var from = StartPos;
		var controlPoint = endPos.WithZ( from.z * 3 );

		for ( var i = 1; i <= SegmentCount; i++ )
		{
			var offset = (float)i / (SegmentCount / 2);
			var position = (float)Math.Pow( 1 - offset, 3 ) * StartPos + 1 * (1 - offset) * offset * controlPoint + (float)Math.Pow( offset, 3 ) * endPos;

			ArcSegment segment = new() { StartPos = from };
			from = position;
			segment.EndPos = from;

			var tr = Trace.Ray( segment.StartPos, segment.EndPos ).Ignore( Owner ).Ignore( Gadget ).Radius( 2f ).Run();

			if ( tr.Hit )
			{
				var travelDir = (tr.StartPosition - tr.EndPosition).Normal;
				segment.EndPos = tr.EndPosition + travelDir;
				segment.HitNormal = tr.Normal;
				segments.Add( segment );

				return segments;
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
		const float epsilon = 0.001f;
		var segments = new List<ArcSegment>();

		var velocity = force * -direction;
		var position = startPos;

		for ( var i = 0; i < SegmentCount; i++ )
		{
			ArcSegment segment = new() { StartPos = position };

			velocity -= new Vector3( windForceX, 0, 0 );
			velocity -= Game.PhysicsWorld.Gravity * epsilon;
			position -= velocity;

			segment.EndPos = position;

			var tr = Trace.Ray( segment.StartPos, segment.EndPos )
				.Ignore( Owner )
				.Ignore( Gadget )
				.Radius( 2f )
				.Run();

			if ( tr.Hit )
			{
				var travelDir = (tr.StartPosition - tr.EndPosition).Normal;
				segment.EndPos = tr.EndPosition + travelDir;
				segment.HitNormal = tr.Normal;
				segments.Add( segment );

				return segments;
			}

			segments.Add( segment );
		}

		return segments;
	}

	public List<ArcSegment> RunTowardsWithBounces( Vector3 direction, float force, float windForceX, int maxBounceQty = 0 )
	{
		List<ArcSegment> segments = new();

		var trace = RunTowards( StartPos, direction, force, windForceX );
		var activeForce = force;

		for ( var i = 0; i < 100; i++ )
		{
			segments.AddRange( trace );

			var traceEnd = trace.Last();
			if ( Vector3.GetAngle( traceEnd.HitNormal, Vector3.Up ) < 45 )
				break;

			activeForce *= 0.66f;

			DebugOverlay.Line( traceEnd.EndPos, traceEnd.EndPos + traceEnd.HitNormal * 10, Color.Red );

			var traceDirection = Vector3.GetAngle( traceEnd.HitNormal, Vector3.Up ) < 45 ? traceEnd.EndPos.Normal + traceEnd.HitNormal : traceEnd.HitNormal;
			trace = RunTowards( traceEnd.EndPos, traceDirection, activeForce, windForceX );

			if ( maxBounceQty > 0 && i >= maxBounceQty )
				break;
		}

		return segments;
	}

	public static void Draw( List<ArcSegment> segments )
	{
		var index = 0;
		foreach ( var segment in segments )
		{
			DebugOverlay.Text( index.ToString(), segment.StartPos );
			DebugOverlay.Line( segment.StartPos, segment.EndPos );

			index++;
		}
	}
}
