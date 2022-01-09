using Sandbox;
using System.Linq;
using Sandbox.UI;
using System;
using System.Collections.Generic;

namespace Grubs.Terrain
{
	public partial class Circle : SDF
	{
		[Net] public Vector2 Center { get; set; }
		[Net] public float Radius { get; set; }
		public override float GetDistance( Vector2 point )
		{
			return Center.Distance( point ) - Radius;
		}

		public Circle() { }

		public Circle( Vector2 center, float radius, MergeType type = MergeType.Add )
		{
			Center = center;
			Radius = radius;
			Type = type;
		}

		public Circle( Vector3 center, float radius, MergeType type = MergeType.Add )
		{
			Center = new Vector2( center.x, center.z );
			Radius = radius;
			Type = type;
		}
	}

	public partial class Capsule : SDF
	{
		[Net] public Vector2 From { get; set; }
		[Net] public Vector2 To { get; set; }
		[Net] public float Radius { get; set; }

		public Capsule() { }

		public Capsule( Vector2 from, Vector2 to, float radius, MergeType type = MergeType.Add )
		{
			From = from;
			To = to;
			Radius = radius;
			Type = type;
		}

		public override float GetDistance( Vector2 point )
		{
			Vector2 pa = point - From, ba = To - From;
			float h = (float)Math.Clamp( Vector2.GetDot( pa, ba ) / Vector2.GetDot( ba, ba ), 0, 1f );
			return (pa - ba * h).Length - Radius;
		}

	}


	public partial class Rectangle : SDF
	{
		[Net] public Vector2 Center { get; set; }
		[Net] public Vector2 Extents { get; set; }

		public Rectangle() { }

		public Rectangle( Vector2 center, Vector2 extents, MergeType type = MergeType.Add )
		{
			Center = center;
			Extents = extents;
			Type = type;
		}

		public override float GetDistance( Vector2 point )
		{
			float x = MathF.Max( point.x - Center.x - Extents.x, Center.x - point.x - Extents.x );
			float y = MathF.Max( point.y - Center.y - Extents.y, Center.y - point.y - Extents.y );

			float d = x;
			d = MathF.Max( d, y );
			return d;
		}

	}
}
