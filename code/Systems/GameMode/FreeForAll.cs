using Grubs.Systems.Pawn;
using Grubs.Systems.Pawn.Grubs;
using Grubs.Terrain;

namespace Grubs.Systems.GameMode;

[Title( "Grubs - Free For All" ), Category( "Grubs/Game Mode" )]
public sealed class FreeForAll : BaseGameMode
{
	public override string Name => "Free For All";
	
	[Sync( SyncFlags.FromHost )]
	public FreeForAllState State { get; set; } = FreeForAllState.Lobby;

	[Sync( SyncFlags.FromHost )]
	private NetList<Player> PlayerQueue { get; set; } = new();
	
	[Sync( SyncFlags.FromHost )]
	public Player ActivePlayer { get; private set; }
	
	[Sync( SyncFlags.FromHost )]
	public TimeUntil TimeUntilTurnOver { get; private set; }
	
	[Sync( SyncFlags.FromHost )]
	public bool TurnIsChanging { get; private set; }
	
	/// <summary>
	/// The amount of time elapsed since we started changing turns.
	/// Used to check for the minimum and maximum turn change wait.
	/// </summary>
	private TimeSince TimeSinceTurnChangeStarted { get; set; }

	private const float MinimumTurnChangeDuration = 0.5f;
	private const float MaximumTurnChangeDuration = 20f;

	protected override void OnModeInit()
	{
		Log.Info( $"{Name} mode initializing." );

		State = FreeForAllState.Lobby;

		GrubsTerrain.Instance.Init();
	}

	protected override void OnModeStarted()
	{
		if ( State is FreeForAllState.Playing )
		{
			Log.Warning( $"Tried to start {Name}, but State is already Playing." );
			return;
		}
		
		Log.Info( $"{Name} mode starting." );


		// For each player, spawn Grubs and initialize their inventory.
		foreach ( var player in Players )
		{
			InitializePlayer( player );
		}

		ActivePlayer = PlayerQueue.First();
		TimeUntilTurnOver = GrubsConfig.TurnDuration;
		State = FreeForAllState.Playing;
	}

	protected override void OnModeUpdate()
	{
		if ( State is FreeForAllState.Playing )
		{
			if ( TimeUntilTurnOver && !TurnIsChanging )
			{
				StartTurnChange();
			}

			if ( TurnIsChanging )
			{
				UpdateTurnChange();
			}
		}
	}

	protected override void OnPlayerJoined( Player player )
	{
		Log.Info( $"Adding {player.GameObject.Name} to Free For All game mode." );

		if ( State is not FreeForAllState.Playing )
			return;

		if ( GrubsConfig.SpawnLateJoiners )
		{
			InitializePlayer( player );
		}
		else
		{
			Log.Warning( "Player joined late. What do we do here?" );
		}
	}

	protected override bool IsGameStarted()
	{
		return State is FreeForAllState.Playing;
	}

	public override bool IsGrubActive( Grub grub )
	{
		return ActivePlayer.ActiveGrub == grub && !TurnIsChanging;
	}

	private void InitializePlayer( Player player )
	{
		const float grubSize = 8f;
		
		if ( !player.IsValid() )
			return;
		
		for ( var i = 0; i < GrubsConfig.GrubCount; i++ )
		{
			var spawnLocation = GrubsTerrain.Instance.FindSpawnLocation( size: grubSize );
			player.AddGrub( spawnLocation );
		}
			
		PlayerQueue.Add( player );
	}

	private void StartTurnChange()
	{
		Log.Info( $"Starting turn change (finished: {ActivePlayer})." );
		TimeSinceTurnChangeStarted = 0;
		TurnIsChanging = true;
	}

	private void UpdateTurnChange()
	{
		if ( TimeSinceTurnChangeStarted < MinimumTurnChangeDuration )
			return;
		
		TurnIsChanging = false;
		TimeUntilTurnOver = GrubsConfig.TurnDuration;
		RotateActivePlayer();
	}

	private void RotateActivePlayer()
	{
		Log.Info( $"Rotating active player (current: {ActivePlayer})." );
		if ( PlayerQueue.Count == 0 )
		{
			foreach ( var player in Player.All )
				PlayerQueue.Add( player );
		}
		
		var nextPlayer = PlayerQueue.First();
		PlayerQueue.Remove( nextPlayer );
		
		Log.Info( $"New active player: {nextPlayer}." );
		ActivePlayer = nextPlayer;
		ActivePlayer?.RotateActiveGrub();
	}

	[ConCmd( "gr_ffa_next_state" )]
	public static void NextStateCmd()
	{
		if ( Current is not FreeForAll ffa )
			return;

		switch ( ffa.State )
		{
			case FreeForAllState.Lobby:
				ffa.Start();
				break;
			case FreeForAllState.Playing:
				ffa.State = FreeForAllState.GameOver;
				break;
			case FreeForAllState.GameOver:
				ffa.State = FreeForAllState.Lobby;
				break;
		}
		Log.Info( $"Set state to {ffa.State}." );
	}
}

public enum FreeForAllState
{
	Lobby,
	Playing,
	GameOver
}
