namespace Grubs;

[GameResource( "Grub Clothing", "gcloth", "A piece of grub clothing that can be equipped." )]
public partial class Cosmetic : Clothing
{
	[Category( "Display Information" )]
	[ResourceType( "png" )]
	public string MenuIcon { get; set; }

	/// <inheritdoc/>
	protected sealed override void PostLoad()
	{
		base.PostLoad();

		Player.CosmeticPresets.Add( this );
	}
}

public partial class Player
{
	public static readonly List<Cosmetic> CosmeticPresets = new();

	/// <summary>
	/// The index of the cosmetic selected from <see cref="CosmeticPresets"/>
	/// "-1" indicating we have nothing selected.
	/// </summary>
	[ConVar.ClientData]
	public int SelectedCosmeticIndex { get; set; }
	public bool HasCosmeticSelected => SelectedCosmeticIndex != -1;

	public static readonly List<Color> ColorPresets = new()
	{
		Color.FromBytes(232, 59, 105),  // Red
		Color.FromBytes(33, 146, 255),  // Blue
		Color.FromBytes(56, 229, 77),   // Green
		Color.FromBytes(56, 118, 29),	// Forest Green
		Color.FromBytes(248, 249, 136), // Yellow
		Color.FromBytes(251, 172, 204), // Pink
		Color.FromBytes(103, 234, 202), // Cyan
		Color.FromBytes(255, 174, 109), // Orange
		Color.FromBytes(173, 162, 255), // Purple
		Color.FromBytes(175, 99, 59),	// Brown
		Color.FromBytes(118, 103, 87),	// Pastel Brown
		Color.FromBytes(240, 236, 211)	// Eggshell
	};

	/// <summary>
	/// The player's active color networked to everyone.
	/// </summary>
	[Net]
	public Color Color { get; private set; } = Color.White;

	/// <summary>
	/// The player's selected color during customization, only networked to the server.
	/// </summary>
	[ConVar.ClientData]
	public Color SelectedColor { get; set; }

	public static readonly List<string> GrubNamePresets = new()
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
		"Matty",
		"Maggie",
		"Weevil",
		"Gub",
		"Jaspy",
		"Borris",
		"Slugma",
		"Ziks"
	};

	/// <summary>
	/// JSON serialized list of strings containing the player's selected grub names.
	/// </summary>
	[ConVar.ClientData]
	public string GrubNames { get; private set; }

	/// <summary>
	/// Clientside names the player selected.
	/// </summary>
	public List<string> SelectedGrubNames { get; private set; } = new();

	/// <summary>
	/// Populates <see cref="SelectedGrubNames"/> with random names of size <see cref="GrubsConfig.GrubCount"/>
	/// </summary>
	public void PopulateGrubNames()
	{
		SelectedGrubNames.Clear();

		for ( int i = 0; i < GrubsConfig.GrubCount; ++i )
			SelectedGrubNames.Add( Random.Shared.FromList( GrubNamePresets ) );

		SerializeGrubNames();
	}

	/// <summary>
	/// Updates <see cref="GrubNames"/> with the serialized version of <see cref="SelectedGrubNames"/>
	/// We do this because we cannot have a [ConVar.ClientData] of type list string
	/// </summary>
	public void SerializeGrubNames()
	{
		GrubNames = System.Text.Json.JsonSerializer.Serialize( SelectedGrubNames );
	}
}
