﻿// using Grubs.Extensions;
// using Grubs.Gamemodes;
//
// namespace Grubs.Pawn;
//
// [Title( "Grubs - Player" ), Category( "Grubs" )]
// public sealed class Player : Component
// {
// 	public static IEnumerable<Player> All => Game.ActiveScene.GetAllComponents<Player>();
//
// 	public bool IsActive => Gamemode.FFA?.ActivePlayerId == Id;
// 	public bool ShouldHaveTurn => GameObject.IsValid() && !IsDead();
// 	public int GetTotalGrubHealth => (int)Grubs.Sum( g => g.ToComponent<Grub>()?.Health.CurrentHealth )?.Clamp( 0, float.MaxValue );
// 	public int GetHealthPercentage => (GetTotalGrubHealth / (1.5f * Grubs.Count)).CeilToInt();
//
// 	public bool HasFiredThisTurn { get; set; }
//
// 	[Sync] public ulong SteamId { get; set; }
// 	[Sync] public string SteamName { get; set; }
//
// 	[Sync] public string SelectedColor { get; set; } = string.Empty;
//
// 	[Sync] public Grub ActiveGrub { get; set; }
//
// 	[Sync] public NetList<Guid> Grubs { get; set; } = new();
//
// 	// Queue<Grub>
// 	[HostSync] public NetList<Guid> GrubQueue { get; set; } = new();
//
// 	[Sync] public float PlayTime { get; set; } = 0;
//
// 	[Property] public required GameObject GrubPrefab { get; set; }
//
// 	[Property] public required PlayerInventory Inventory { get; set; }
//
// 	[Property] public required PlayerVoice Voice { get; set; }
//
// 	[Property] public required GameObject TurnIndicatorPrefab { get; set; }
//
// 	[Property, ReadOnly] public Vector3 MousePosition { get; set; }
//
// 	private static readonly Plane _plane = new( new Vector3( 0f, 512f, 0f ), Vector3.Left );
//
// 	protected override void OnStart()
// 	{
// 		SteamId = Network.Owner.SteamId;
// 		SteamName = Network.Owner.DisplayName;
//
// 		if ( IsProxy )
// 			return;
//
// 		if ( TurnIndicatorPrefab is not null )
// 			TurnIndicatorPrefab.Clone();
//
// 		SelectedColor = GrubsConfig.PresetTeamColors.Values
// 			.OrderBy( _ => Guid.NewGuid() )
// 			.FirstOrDefault( color => !All.Any( p => p.SelectedColor == color ) );
//
// 		if ( string.IsNullOrEmpty( SelectedColor ) )
// 		{
// 			SelectedColor = Color.White.Hex;
// 			Log.Warning( "We couldn't find an available team color. Please report this to an Ape!" );
// 			Log.Warning( "(Failover) Your team color has been set to white." );
// 		}
//
// 		_ = Fetch();
// 	}
//
// 	private async Task Fetch()
// 	{
// 		var pkg = await Package.FetchAsync( "apetavern.grubs", false );
// 		PlayTime = pkg.Interaction.Seconds / 3600f;
// 	}
//
// 	protected override void OnUpdate()
// 	{
// 		var cursorRay = Scene.Camera.ScreenPixelToRay( Input.UsingController
// 			? new Vector2( Screen.Width / 2, Screen.Height / 2 )
// 			: Mouse.Position );
// 		var endPos = _plane.Trace( cursorRay, twosided: true );
// 		MousePosition = endPos ?? new Vector3( 0f, 512f, 0f );
// 	}
//
// 	[Authority]
// 	public void OnTurn()
// 	{
// 		Sound.Play( "ui_turn_indicator" );
// 	}
//
// 	public void EndTurn()
// 	{
// 		Inventory.Holster( Inventory.ActiveSlot );
// 		HasFiredThisTurn = false;
// 	}
//
// 	public IEnumerable<Grub> GetOwnedGrubs()
// 	{
// 		return Scene.GetAllComponents<Grub>()
// 			.Where( g => g.IsValid && g.Network.Owner == Network.Owner );
// 	}
//
// 	public bool IsDead()
// 	{
// 		return GetOwnedGrubs().All( g => g.IsDead );
// 	}
//
// 	[Rpc.Broadcast]
// 	public void Cleanup()
// 	{
// 		Inventory.Cleanup();
// 		Grubs.Clear();
//
// 		foreach ( var grub in GetOwnedGrubs() )
// 		{
// 			grub.GameObject.Destroy();
// 		}
// 	}
// }
