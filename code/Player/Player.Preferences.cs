namespace Grubs;

[Prefab, Category( "Cosmetic" )]
public partial class Cosmetic : AnimatedEntity
{
	[Prefab, Net, ResourceType( "png" )]
	public string Icon { get; set; }

	public static IList<Cosmetic> GetCosmetics()
	{
		var cosmetics = new List<Cosmetic>();
		var cosmeticPrefabs = ResourceLibrary.GetAll<Prefab>()
			.Where( x => x is not null
				&& x.Root is not null
				&& TypeLibrary.GetType( x.Root.Class ).TargetType == typeof( Cosmetic )
			);

		foreach ( var prefab in cosmeticPrefabs )
		{
			Assert.True( PrefabLibrary.TrySpawn<Cosmetic>( prefab.ResourcePath, out var cosmetic ) );
			cosmetics.Add( cosmetic );
		}

		return cosmetics;
	}
}

public partial class Player
{
	[Net]
	public IList<Cosmetic> CosmeticPresets { get; set; }

	/// <summary>
	/// The index of the cosmetic selected from <see cref="CosmeticPresets"/>
	/// "-1" indicating we want to use client clothes.
	/// </summary>
	[ConVar.ClientData]
	public int SelectedCosmeticIndex { get; set; }
	public bool HasCosmeticSelected => SelectedCosmeticIndex != -1;

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

	/// <summary>
	/// The player's active color networked to everyone.
	/// </summary>
	[Net]
	public Color Color { get; private set; }

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
		"Slugma"
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
