using Grubs.Extensions;
using Grubs.Gamemodes;
using Grubs.UI;

namespace Grubs.Pawn;

[Title( "Grubs - Player" ), Category( "Grubs" )]
public sealed class Player : Component
{
	public bool IsActive => Gamemode.FFA?.ActivePlayerId == Id;
	public bool ShouldHaveTurn => GameObject.IsValid() && !IsDead();
	public Grub ActiveGrub { get; set; }

	public bool HasFiredThisTurn { get; set; }

	[Sync] public string SelectedColor { get; set; } = "";

	[Sync] public Guid ActiveGrubId { get; set; }

	[Sync] public NetList<Guid> Grubs { get; set; } = new();

	[Property] public required GameObject GrubPrefab { get; set; }

	[Property] public required PlayerInventory Inventory { get; set; }

	[Property, ReadOnly] public Vector3 MousePosition { get; set; }

	private static readonly Plane _plane = new( new Vector3( 0f, 512f, 0f ), Vector3.Left );

	protected override void OnStart()
	{
		SelectedColor = Color.Random.Hex;
	}

	protected override void OnUpdate()
	{
		var cursorRay = Scene.Camera.ScreenPixelToRay( Mouse.Position );
		var endPos = _plane.Trace( cursorRay, twosided: true );
		MousePosition = endPos ?? new Vector3( 0f, 512f, 0f );
	}

	public void EndTurn()
	{
		Inventory.Holster( Inventory.ActiveSlot );
		HasFiredThisTurn = false;
	}

	public IEnumerable<Grub> GetOwnedGrubs()
	{
		return Scene.GetAllComponents<Grub>()
			.Where( g => g.IsValid && g.Network.OwnerConnection == Network.OwnerConnection );
	}

	public bool IsDead()
	{
		return GetOwnedGrubs().All( g => g.IsDead );
	}
}
