using Grubs.Common;
using Grubs.Equipment.Weapons;
using Grubs.Systems.GameMode;
using Grubs.Systems.Pawn.Grubs;

namespace Grubs.Systems.Pawn;

[Title( "Player" ), Category( "Grubs/Pawn" )]
public sealed class Player : LocalComponent<Player>
{
	private static readonly Logger Log = new( "Player" );
	
	public static IEnumerable<Player> All => Game.ActiveScene.GetAllComponents<Player>();
	public static IEnumerable<Player> AllLiving => All.Where( p => p.IsPlaying && !p.IsDead );
	
	// [Sync( SyncFlags.FromHost ), Property, ReadOnly]
	// public Client Client { get; set; }

	[Sync( SyncFlags.FromHost )] 
	public NetList<Grub> Grubs { get; } = new();
	
	[Sync( SyncFlags.FromHost )]
	public Grub ActiveGrub { get; private set; }
	
	[Property]
	public GameObject GrubPrefab { get; private set; }
	
	[Property]
	public Inventory Inventory { get; private set; }

	[Sync( SyncFlags.FromHost )]
	public PlayerColor PlayerColor { get; set; } = PlayerColor.Khaki;
	
	[Sync( SyncFlags.FromHost )]
	public bool IsPlaying { get; set; }
	
	[Sync]
	public bool HasFiredThisTurn { get; private set; }

	public bool IsActive => BaseGameMode.Current.IsPlayerActive( this );
	public bool IsDead => Grubs.Count == 0;
	
	private int GetTotalGrubHealth => (int)Grubs.Sum( g =>
	{
		if ( g?.Health?.CurrentHealth != null ) 
			return g.Health.CurrentHealth.Clamp( 1, float.MaxValue );
		return 0;
	} ).Clamp( 0, float.MaxValue );
	public int GetHealthPercentage => (GetTotalGrubHealth / (1.5f * BaseGameMode.Current.GrubCount)).CeilToInt();

	public Vector3 MousePosition { get; private set; }
	private static readonly Plane Plane = 
		new( new Vector3( 0f, 512f, 0f ), Vector3.Left );


	protected override void OnStart()
	{
		if ( IsProxy )
			return;

		Local = this;
	}

	protected override void OnUpdate()
	{
		var cursorRay = Scene.Camera.ScreenPixelToRay( Input.UsingController
			? new Vector2( Screen.Width / 2, Screen.Height / 2 )
			: Mouse.Position );
		var endPos = Plane.Trace( cursorRay, twosided: true );
		MousePosition = endPos ?? new Vector3( 0f, 512f, 0f );
	}

	public void AddGrub( Vector3 spawnPosition )
	{
		Log.Info( $"Adding new grub for player {Network.Owner.DisplayName} at {spawnPosition}." );
		
		var grubObj = GrubPrefab.Clone();
		grubObj.WorldPosition = spawnPosition;
		grubObj.Network.SetOrphanedMode( NetworkOrphaned.Host );
		grubObj.NetworkSpawn( Network.Owner );
		
		var grub = grubObj.GetComponent<Grub>();
		grub.SetOwner( this );
		
		Grubs.Insert( 0, grub );

		if ( ActiveGrub is null )
		{
			RotateActiveGrub();
		}
		
		Log.Info( $"Created {grub}." );
	}

	public void OnGrubDied( Grub grub )
	{
		if ( Grubs.Remove( grub ) )
		{
			Log.Info( $"Removed {grub} from {this}." );
		}
	}

	public void RotateActiveGrub()
	{
		Log.Info( $"Rotating active grub {ActiveGrub}" );

		if ( Grubs.Count == 0 )
		{
			Log.Warning( "Trying to rotate grubs on a player with no grubs!" );
			return;
		}
		
		var nextGrub = Grubs.First();
		Grubs.Remove( nextGrub );
		Grubs.Add( nextGrub );
		Log.Info( $"New active grub: {nextGrub}." );

		ActiveGrub = nextGrub;
	}

	public void OnFired()
	{
		HasFiredThisTurn = true;
	}

	public void Cleanup()
	{
		Grubs.Clear();
		ActiveGrub = null;
		IsPlaying = false;
		
		CleanupRpc();
	}

	[Rpc.Owner( NetFlags.HostOnly )]
	private void CleanupRpc()
	{
		HasFiredThisTurn = false;
		Inventory.Cleanup();
	}

	[Rpc.Owner( NetFlags.HostOnly )]
	public void OnTurnStart( Grub grub )
	{
		Sound.Play( "ui_turn_indicator" );
		
		if ( grub.IsValid() )
			grub.OnTurnStart();
	}

	[Rpc.Owner( NetFlags.HostOnly )]
	public void OnTurnEnd()
	{
		HasFiredThisTurn = false;
		Inventory?.HolsterActive();
	}
}
