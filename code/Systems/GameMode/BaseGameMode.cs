using Grubs.Systems.Pawn;

namespace Grubs.Systems.GameMode;

public abstract class BaseGameMode : Component
{
	public static BaseGameMode Current { get; private set; }
	public virtual string Name => "Game Mode";
	
	[Sync( SyncFlags.FromHost )] public NetList<Player> Players { get; set; } = new();
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
	
	public void HandlePlayerJoined( Player player )
	{
		if ( !Networking.IsHost )
			return;
		
		Players.Add( player );
		OnPlayerJoined( player );
	}
	
	/// <summary>
	/// Called after the player is added to the Players list.
	/// </summary>
	protected virtual void OnPlayerJoined( Player player ) { }

	protected virtual bool IsGameStarted()
	{
		return false;
	}
}
