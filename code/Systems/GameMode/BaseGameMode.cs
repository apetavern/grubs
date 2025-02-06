using Grubs.Systems.Pawn;
using Grubs.Systems.Pawn.Grubs;

namespace Grubs.Systems.GameMode;

public abstract class BaseGameMode : Component
{
	public static BaseGameMode Current { get; private set; }
	public virtual string Name => "Game Mode";

	protected virtual int MaxPlayers => 4;
	
	[Sync( SyncFlags.FromHost )] 
	protected NetList<Player> Players { get; private set; } = new();
	
	[Sync( SyncFlags.FromHost )]
	public int GrubCount { get; set; }
	
	[Sync( SyncFlags.FromHost )]
	public bool IsSandboxMode { get; set; }
	
	public bool GameStarted => IsGameStarted();

	protected override void OnStart()
	{
		Current = this;
		
		if ( !Networking.IsHost )
			return;
		
		Init();
	}

	private void Init()
	{
		if ( !Networking.IsHost )
			return;
		
		OnModeInit();
	}
	
	protected virtual void OnModeInit() { }

	public void Start()
	{
		if ( !Networking.IsHost )
			return;
		
		OnModeStarted();
	}
	
	protected virtual void OnModeStarted() { }

	protected override void OnUpdate()
	{
		if ( !Networking.IsHost )
			return;
		
		OnModeUpdate();
	}
	
	protected virtual void OnModeUpdate() { }

	public void HandlePlayerJoined( Player player )
	{
		if ( !Networking.IsHost )
			return;

		if ( Players.Count >= MaxPlayers )
		{
			Log.Warning( "Max players exceeded!" );
			return;
		}

		AssignPlayerColor( player );
		Players.Add( player );
		OnPlayerJoined( player );
	}

	public void HandlePlayerLeft( Player player )
	{
		if ( !Networking.IsHost )
			return;
		
		OnPlayerLeft( player );
	}

	private void AssignPlayerColor( Player player )
	{
		var takenColors = Players.Select( p => p.PlayerColor );
		var allColors = Enum.GetValues<PlayerColor>().ToList();

		var selectedColor = allColors.First( a => !takenColors.Contains( a ) );
		Log.Info( $"Assigning player color {selectedColor.ToString()} to player {player.Network.Owner.DisplayName}." );
		player.PlayerColor = selectedColor;
	}
	
	/// <summary>
	/// Called after the player is added to the Players list.
	/// </summary>
	protected virtual void OnPlayerJoined( Player player ) { }
	
	/// <summary>
	/// Called after the player disconnects from the lobby.
	/// </summary>
	protected virtual void OnPlayerLeft( Player player ) { }

	/// <summary>
	/// Call for when a Grub is damaged.
	/// </summary>
	public void GrubDamaged( Grub grub )
	{
		OnGrubDamaged( grub );
	}

	/// <summary>
	/// Call for when a Grub dies.
	/// </summary>
	/// <param name="grub">The dead grub :sob:</param>
	public void GrubDied( Grub grub )
	{
		OnGrubDied( grub );
	}

	/// <summary>
	/// Call when Equipment is used.
	/// </summary>
	public void EquipmentUsed( Equipment.Equipment equipment )
	{
		OnEquipmentUsed( equipment );
	}

	protected virtual void OnGrubDamaged( Grub grub ) { }
	
	protected virtual void OnGrubDied( Grub grub ) { }

	protected virtual void OnEquipmentUsed( Equipment.Equipment equipment ) { }

	protected virtual bool IsGameStarted()
	{
		return false;
	}

	public virtual bool IsPlayerActive( Player player )
	{
		return false;
	}

	public virtual bool IsGrubActive( Grub grub )
	{
		return false;
	}
}
