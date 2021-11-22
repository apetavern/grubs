using Sandbox;

namespace TerryForm.Utils
{
	public struct Range
	{
		public float Min { get; set; }
		public float Max { get; set; }

		public float Clamp( float t ) => t.Clamp( Min, Max );

		public Range( float min, float max )
		{
			Min = min;
			Max = max;
		}
	}
}
