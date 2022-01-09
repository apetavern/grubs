using Sandbox;
using System.Linq;
using Sandbox.UI;
using System;
using System.Collections.Generic;
using Sandbox.Internal.Globals;

namespace Grubs.Terrain
{
	public class Stopwatch
	{
		private DateTime startTime;
		public Stopwatch() { startTime = DateTime.Now; }
		public double Stop() => (DateTime.Now - startTime).TotalMilliseconds;
		public double Lap()
		{
			double stopTime = Stop();
			Restart();
			return stopTime;
		}
		public void Restart() => startTime = DateTime.Now;
	}

	public static class DebugOverlayExtension
	{
		public static void LineCircle( this DebugOverlay dol, Vector3 pos, Rotation rot, float radius, float time, Color color )
		{
			float step = (2f * MathF.PI) / 64f;
			Vector3 right = rot.Right;
			Vector3 up = rot.Up;

			Vector3 prevLine = Vector3.Zero;
			for ( int i = 0; i < 64; i++ )
			{
				float rad = step * i;
				float cos = MathF.Cos( rad );
				float sin = MathF.Sin( rad );

				Vector3 line = pos + (right * cos + up * sin) * radius;
				if ( i > 0 )
					dol.Line( prevLine, line, color, time );
				prevLine = line;
			}
		}
	}
}
