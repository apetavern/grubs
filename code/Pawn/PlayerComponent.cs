using Grubs.Gamemodes;
using Grubs.UI;

namespace Grubs.Pawn;

[Title( "Grubs - Player" ), Category( "Grubs" )]
public sealed class Player : Component
{
	public bool IsActive => Gamemode.FFA?.ActivePlayerId == Id;
	public Grub? ActiveGrub { get; set; }

	[Sync] public string SelectedColor { get; set; } = Color.Random.Hex;

	[Property] public required GameObject GrubPrefab { get; set; }

	[Property] public required PlayerInventory Inventory { get; set; }
}
