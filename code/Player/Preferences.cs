namespace Grubs;

public partial class Preferences : EntityComponent<Player>
{
	public static readonly List<Color> ColorPresets = new()
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

	// Clientside
	public Color SelectedColor { get; set; }

	public static readonly List<string> GrubNames = new()
	{
		"Froggy",
		"Balls",
		"Boggy",
		"Spicy",
		"Hot",
		"Pinky",
		"Perky",
		"Gumby",
		"Dick",
		"Panini",
		"Wilson",
		"Winky",
		"Cammy",
		"Bakky",
		"Avoofo",
		"Gibby",
		"Matty"
	};

	// Clientside
	public List<string> SelectGrubNames = new();

	public void SetDefaultGrubNames()
	{
		SelectGrubNames.Clear();

		for ( int i = 0; i < GrubsConfig.GrubCount; ++i )
			SelectGrubNames.Add( Random.Shared.FromList( GrubNames ) );
	}

	protected override void OnActivate()
	{
		SelectedColor = Random.Shared.FromList( ColorPresets );
		SetDefaultGrubNames();
	}
}
