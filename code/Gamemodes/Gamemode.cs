﻿namespace Grubs;

[Category( "Grubs" )]
public partial class Gamemode : Entity
{
	public virtual string GamemodeName => "";

	/// <summary>
	/// A list of currently connected players that are actively apart of the gamemode.
	/// </summary>
	[Net]
	public IList<Player> Players { get; set; }

	private List<Player> DisconnectedPlayers { get; set; } = new();

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

	public override void Simulate( IClient client )
	{
		foreach ( var disconnectedPlayer in DisconnectedPlayers )
		{
			disconnectedPlayer.Simulate( client );
		}
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

	internal virtual void OnClientJoined( IClient client ) { }

	internal virtual void OnClientDisconnect( IClient cl, NetworkDisconnectionReason reason )
	{
		if ( cl.Pawn is Player player )
			DisconnectedPlayers.Add( player );
	}

	internal virtual void PrepareLoadout( Player player, Inventory inventory ) { }

	internal virtual void OnPlayerKilled( Player player ) { }

	internal virtual void OnWeaponDropped( Player player, Weapon weapon ) { }

	internal virtual void MoveToSpawnpoint( IClient client ) { }

	internal virtual void UseTurn( bool giveMovementGrace = false ) { }
}
