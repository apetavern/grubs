using Grubs.Pawn;
using Grubs.Systems.Pawn.Grubs;

namespace Grubs.Helpers;

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
	private int SegmentCount { get; set; } = 256;
	private Grub Grub { get; set; }

	public ArcTrace( Grub source, Vector3 startPos )
	{
		Grub = source;
		StartPos = startPos;
	}

	public List<ArcSegment> RunTo( Scene scene, Vector3 endPos )
	{
		var segments = new List<ArcSegment>();
		var from = StartPos;
		var controlPoint = endPos.WithZ( from.z * 3 );

		for ( var i = 1; i <= SegmentCount; i++ )
		{
			var offset = i / (SegmentCount / 2f);
			var position = MathF.Pow( 1 - offset, 3 ) * StartPos + 1 * (1 - offset) * offset * controlPoint +
			               MathF.Pow( offset, 3 ) * endPos;

			ArcSegment segment = new() { StartPos = from };
			from = position;
			segment.EndPos = from;

			var tr = scene.Trace.Ray( segment.StartPos, segment.EndPos )
				.IgnoreGameObjectHierarchy( Grub.GameObject )
				.WithoutTags( "dead" )
				.Radius( 4f )
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

	public List<ArcSegment> RunTowards( Scene scene, Vector3 direction, float force, float windForceX )
	{
		return RunTowards( scene, StartPos, direction, force, windForceX );
	}

	/// <summary>
	/// Run a trace specifying the origin, direction, force and wind force.
	/// </summary>
	public List<ArcSegment> RunTowards( Scene scene, Vector3 startPos, Vector3 direction, float force,
		float windForceX )
	{
		const float epsilon = 0.001f;
		var segments = new List<ArcSegment>();

		var velocity = force * -direction;
		var position = startPos;

		for ( var i = 0; i < SegmentCount; i++ )
		{
			ArcSegment segment = new() { StartPos = position };

			velocity -= new Vector3( windForceX / 2, 0, 0 );
			velocity -= scene.PhysicsWorld.Gravity * epsilon;
			position -= velocity;

			segment.EndPos = position;

			var tr = scene.Trace.Ray( segment.StartPos, segment.EndPos )
				.IgnoreGameObjectHierarchy( Grub.GameObject )
				.WithoutTags( "dead" )
				.Size( 1f )
				.Radius( 4f )
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

	public List<ArcSegment> RunTowardsWithBounces( Scene scene, Vector3 direction, float force, float windForceX,
		int maxBounceQty = 0 )
	{
		List<ArcSegment> segments = new();

		var trace = RunTowards( scene, StartPos, direction, force, windForceX );
		var activeForce = force;

		for ( var i = 0; i < 100; i++ )
		{
			segments.AddRange( trace );

			var traceEnd = trace.Last();
			if ( Vector3.GetAngle( traceEnd.HitNormal, Vector3.Up ) < 45 )
				break;

			activeForce *= 0.66f;

			var traceDirection = Vector3.GetAngle( traceEnd.HitNormal, Vector3.Up ) < 45
				? traceEnd.EndPos.Normal + traceEnd.HitNormal
				: traceEnd.HitNormal;
			trace = RunTowards( scene, traceEnd.EndPos, traceDirection, activeForce, windForceX );

			if ( maxBounceQty > 0 && i >= maxBounceQty )
				break;
		}

		return segments;
	}
}
