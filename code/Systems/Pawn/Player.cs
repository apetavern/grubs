using Grubs.Common;
using Grubs.Systems.GameMode;
using Grubs.Systems.Pawn.Grubs;

namespace Grubs.Systems.Pawn;

[Title( "Player" ), Category( "Grubs/Pawn" )]
public sealed class Player : LocalComponent<Player>
{
	public static IEnumerable<Player> All => Game.ActiveScene.GetAllComponents<Player>();
	
	[Sync( SyncFlags.FromHost )]
	public Client Client { get; private set; }

	[Sync( SyncFlags.FromHost )] 
	public NetList<Grub> Grubs { get; } = new();
	
	[Sync( SyncFlags.FromHost )]
	public Grub ActiveGrub { get; private set; }
	
	[Property]
	public GameObject GrubPrefab { get; private set; }
	
	[Property]
	public Inventory Inventory { get; private set; }
	
	[Sync]
	public bool HasFiredThisTurn { get; set; }

	public bool IsActive => BaseGameMode.Current.IsPlayerActive( this );

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

	public void SetClient( Client client )
	{
		Client = client;
	}

	public void AddGrub( Vector3 spawnPosition )
	{
		Log.Info( $"Adding new grub for player {Client} at {spawnPosition}." );
		
		var grubObj = GrubPrefab.Clone();
		grubObj.WorldPosition = spawnPosition;
		grubObj.Network.SetOrphanedMode( NetworkOrphaned.Host );
		grubObj.NetworkSpawn( Client.Connection );
		
		var grub = grubObj.GetComponent<Grub>();
		grub.SetOwner( this );
		
		Grubs.Insert( 0, grub );

		if ( ActiveGrub is null )
		{
			RotateActiveGrub();
		}
		
		Log.Info( $"Created {grub}." );
	}

	public void RotateActiveGrub()
	{
		Log.Info( $"Rotating active grub {ActiveGrub}" );
		
		var nextGrub = Grubs.First();
		Grubs.Remove( nextGrub );
		Grubs.Add( nextGrub );
		Log.Info( $"New active grub: {nextGrub}." );

		ActiveGrub = nextGrub;
	}
}
