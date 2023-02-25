namespace Grubs;

public partial class Preferences : EntityComponent<Player>
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
	public Color Color { get; set; }

	protected override void OnActivate()
	{
		SetColor();
	}

	// TODO: Players should be able to choose a color preset not in use via the menu.
	private void SetColor()
	{
		Color = ColorPresets[ColorIndex++];

		if ( ColorIndex >= ColorPresets.Count )
			ColorIndex = 0;
	}
}
