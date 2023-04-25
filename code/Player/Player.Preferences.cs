namespace Grubs;

[Prefab, Category( "Cosmetic" )]
public partial class Cosmetic : AnimatedEntity
{
	[Prefab, ResourceType( "png" )]
	public string Icon { get; set; }

	public static IEnumerable<Prefab> GetCosmeticPrefabs()
	{
		return ResourceLibrary.GetAll<Prefab>()
			.Where( x => x is not null
				&& x.Root is not null
				&& TypeLibrary.GetType( x.Root.Class ).TargetType == typeof( Cosmetic )
			);
	}
}

public partial class Player
{
	public static readonly List<Cosmetic> CosmeticPresets = new();

	public Cosmetic SelectedCosmetic { get; set; }

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
	public Color ActiveColor { get; private set; }

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
		"Matty"
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
	public void SetDefaultGrubNames()
	{
		SelectedGrubNames.Clear();

		for ( int i = 0; i < GrubsConfig.GrubCount; ++i )
			SelectedGrubNames.Add( Random.Shared.FromList( GrubNamePresets ) );

		SerializeGrubNames();
	}

	public void SerializeGrubNames()
	{
		GrubNames = System.Text.Json.JsonSerializer.Serialize( SelectedGrubNames );
	}

	public void PopulateDefaultPreferences()
	{
		if ( !IsLocalPawn )
			return;

		SelectedColor = Random.Shared.FromList( ColorPresets );
		SetDefaultGrubNames();

		foreach ( var prefab in Cosmetic.GetCosmeticPrefabs() )
		{
			Assert.True( PrefabLibrary.TrySpawn<Cosmetic>( prefab.ResourcePath, out var cosmetic ) );
			CosmeticPresets.Add( cosmetic );
		}
	}
}
