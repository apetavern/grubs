namespace Grubs.Utils;

/// <summary>
/// Utility struct that defines a range of floats and useful behaviour associated with the range.
/// </summary>
public struct FloatRange
{
	public float Min { get; set; }
	public float Max { get; set; }

	public float Clamp( float t ) => t.Clamp( Min, Max );
	public float LerpInverse( float t ) => t.LerpInverse( Min, Max );

	public FloatRange( float min, float max )
	{
		Min = min;
		Max = max;
	}
}
