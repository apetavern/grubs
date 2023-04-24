namespace Grubs;

public partial class Preferences : EntityComponent<Player>
{
	public readonly List<Color> ColorPresets = new()
	{
		Color.FromBytes(232, 59, 105),  // Red
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
		Color = Random.Shared.FromList( ColorPresets );
	}
}
