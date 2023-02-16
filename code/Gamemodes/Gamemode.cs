namespace Grubs;

[Category( "Grubs" )]
public partial class Gamemode : Entity
{
	public virtual string GamemodeName => "";

	[Net]
	public int PlayerCount { get; private set; }

	/// <summary>
	/// All players in the game, including dead players.
	/// Excludes disconnected players.
	/// </summary>
	[Net]
	public IList<Player> PlayerList { get; set; }

	[Net]
	public Player ActivePlayer { get; set; }

	[Net]
	public Entity CameraTarget { get; set; }

	[Net]
	public World GameWorld { get; set; }

	/// <summary>
	/// Whether or not the turn is currently changing.
	/// </summary>
	[Net]
	public bool TurnIsChanging { get; set; }

	/// <summary>
	/// Whether or not the current player has used their turn.
	/// </summary>
	[Net]
	public bool UsedTurn { get; set; }

	public bool Initialized { get; set; } = false;

	/// <summary>
	/// Whether or not movement is currently allowed.
	/// </summary>
	[Net]
	public bool AllowMovement { get; set; }

	public virtual bool AllowDamage => true;
	public virtual bool AllowFriendlyFire => false;

	public virtual int MinimumPlayers => 2;

	public override void Spawn()
	{
		Transmit = TransmitType.Always;
	}

	public virtual string GetGameStateLabel()
	{
		return "";
	}

	public virtual float GetTimeRemaining()
	{
		return -1;
	}

	internal virtual void Initialize()
	{
		if ( Initialized )
			return;

		Initialized = true;
	}

	internal virtual void Start() { }

	internal virtual void OnClientJoined( IClient client )
	{
		PlayerCount++;
	}

	internal virtual void OnClientDisconnect( IClient cl, NetworkDisconnectionReason reason )
	{
		PlayerCount--;
	}

	internal virtual void PrepareLoadout( Player player, Inventory inventory ) { }

	internal virtual void OnPlayerKilled( Player player ) { }

	internal virtual void OnWeaponDropped( Player player, Weapon weapon ) { }

	internal virtual void MoveToSpawnpoint( IClient client ) { }

	internal virtual void UseTurn( bool giveMovementGrace = false ) { }
}
