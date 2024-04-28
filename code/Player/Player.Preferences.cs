namespace Grubs;

[GameResource( "Grub Clothing", "gcloth", "A piece of grub clothing that can be equipped." )]
public partial class Cosmetic : Clothing
{
	[Category( "Cloud Info" )]
	public string CloudIdent { get; set; } = "";

	[Category( "Display Information" )]
	[ResourceType( "png" )]
	public string MenuIcon { get; set; }

	/// <inheritdoc/>
	protected sealed override void PostLoad()
	{
		base.PostLoad();

		if ( CloudIdent != "" )
		{
			Package pck = Package.Fetch( CloudIdent, false ).Result;
			this.MenuIcon = pck.Thumb;

			WaitForMount( pck );
		}

		Player.CosmeticPresets.Add( this );
	}

	public async void WaitForMount( Package package )
	{
		await package.MountAsync();
		this.Model = package.GetMeta( "PrimaryAsset", "" );
	}
}

public partial class Player
{
	[Net] public float PlayTime { get; set; } = 0f;

	public static readonly List<Cosmetic> CosmeticPresets = new();

	/// <summary>
	/// The index of the cosmetic selected from <see cref="CosmeticPresets"/>
	/// "-1" indicating we have nothing selected.
	/// </summary>
	[ConVar.ClientData]
	public int SelectedCosmeticIndex { get; set; }
	public bool HasCosmeticSelected => SelectedCosmeticIndex != -1;

	/// <summary>
	/// The player's active color networked to everyone.
	/// </summary>
	[Net]
	public Color Color { get; set; } = DefaultColor;

	public static Color DefaultColor = Color.White;

	[ConCmd.Server]
	public static void SelectColor( uint raw )
	{
		if ( GamemodeSystem.Instance.CurrentState != Gamemode.State.MainMenu )
			return;

		var caller = ConsoleSystem.Caller;
		if ( caller.Pawn is not Player player )
			return;

		var selectedColor = new Color( raw );
		if ( !GrubsGame.Instance.PlayerColors.ContainsKey( selectedColor ) || GrubsGame.Instance.PlayerColors[selectedColor] )
			return;

		if ( player.Color != DefaultColor )
			GrubsGame.Instance.PlayerColors[player.Color] = false;

		GrubsGame.Instance.PlayerColors[selectedColor] = true;
		player.Color = selectedColor;
	}

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

		if ( !FileSystem.Data.FileExists( "GrubNames.txt" ) )
		{
			for ( int i = 0; i < GrubsConfig.GrubCount; ++i )
				SelectedGrubNames.Add( Random.Shared.FromList( GrubNamePresets ) );
		}
		else
		{
			GrubNames = FileSystem.Data.ReadAllText( "GrubNames.txt" );
			SelectedGrubNames = System.Text.Json.JsonSerializer.Deserialize<List<string>>( GrubNames );

			// If we have too many saved, just grab the grub count amount.
			if ( SelectedGrubNames.Count >= GrubsConfig.GrubCount )
				SelectedGrubNames = SelectedGrubNames.GetRange( 0, GrubsConfig.GrubCount );

			// If we don't have enough names, fetch random ones from the preset.
			while ( SelectedGrubNames.Count < GrubsConfig.GrubCount )
				SelectedGrubNames.Add( Random.Shared.FromList( GrubNamePresets ) );

		}

		SerializeGrubNames();
	}

	/// <summary>
	/// Updates <see cref="GrubNames"/> with the serialized version of <see cref="SelectedGrubNames"/>
	/// We do this because we cannot have a [ConVar.ClientData] of type list string
	/// </summary>
	public void SerializeGrubNames()
	{
		GrubNames = System.Text.Json.JsonSerializer.Serialize( SelectedGrubNames );
		FileSystem.Data.WriteAllText( "GrubNames.txt", GrubNames );
	}

	[GrubsEvent.Game.Start]
	void OnGameStart()
	{
		if ( Color == DefaultColor )
			GrubsGame.Instance.TryAssignUnusedColor( this );

	}
}
