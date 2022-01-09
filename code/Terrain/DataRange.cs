using Sandbox;
using System.Linq;
using Sandbox.UI;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Grubs.Terrain
{
	public struct DataRange
	{
		public int MinX { get; set; }
		public int MaxX { get; set; }
		public int MinY { get; set; }
		public int MaxY { get; set; }

		public DataRange( int minX, int maxX, int minY, int maxY )
		{
			MinX = minX;
			MaxX = maxX;
			MinY = minY;
			MaxY = maxY;
		}

		public DataRange ClampedToQuadrant( int quadrant )
		{
			int midX = (MinX + MaxX) / 2;
			int midY = (MinY + MaxY) / 2;

			bool upperHalf = quadrant < 2;
			bool rightHalf = quadrant == 1 || quadrant == 2;

			if ( rightHalf )
				MinX = midX;
			else
				MaxX = midX;

			if ( upperHalf )
				MinY = midY;
			else
				MaxY = midY;

			return this;
		}

		public override string ToString()
		{
			return $"{MinX}-{MaxX}, {MinY}-{MaxY}";
		}
	}
}
