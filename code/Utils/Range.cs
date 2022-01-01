using Sandbox;

namespace Grubs.Utils
{
	public struct Range
	{
		public float Min { get; set; }
		public float Max { get; set; }

		public float Clamp( float t ) => t.Clamp( Min, Max );
		public float LerpInverse( float t ) => t.LerpInverse( Min, Max );

		public Range( float min, float max )
		{
			Min = min;
			Max = max;
		}
	}
}
