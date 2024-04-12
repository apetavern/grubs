namespace Grubs.Pawn;

[Title( "Grubs - Player" ), Category( "Grubs" )]
public sealed class Player : Component
{
	public Grub? ActiveGrub { get; set; }

	[Property] public required GameObject GrubPrefab { get; set; }

	[Property] public required PlayerInventory Inventory { get; set; }
}
