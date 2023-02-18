namespace Grubs;

/// <summary>
/// Attached to every single client on connect.
/// </summary>
public partial class Preferences : EntityComponent
{
	private static int ColorIndex = 0;
	private readonly List<Color> ColorPresets = new()
	{
		Color.FromBytes(246, 90, 131),  // Red
		Color.FromBytes(33, 146, 255),  // Blue
		Color.FromBytes(56, 229, 77),   // Green
		Color.FromBytes(248, 249, 136), // Yellow
		Color.FromBytes(251, 172, 204), // Pink
		Color.FromBytes(103, 234, 202), // Cyan
		Color.FromBytes(255, 174, 109), // Orange
		Color.FromBytes(173, 162, 255), // Purple
	};

	[Net]
	public Color Color { get; set; } = Color.Transparent;

	public void SetColor()
	{
		Color = ColorPresets[ColorIndex++];
	}
}
