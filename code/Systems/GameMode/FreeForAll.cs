using Grubs.Systems.Pawn;
using Grubs.Systems.Pawn.Grubs;
using Grubs.Terrain;

namespace Grubs.Systems.GameMode;

[Title( "Grubs - Free For All" ), Category( "Grubs/Game Mode" )]
public sealed class FreeForAll : BaseGameMode
{
	private static readonly Logger Log = new( "FreeForAll" );
	
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

	private Queue<Grub> DamageQueue { get; } = new();
	private Grub ActiveDamagedGrub { get; set; }
	private TimeUntil TimeUntilNextDamagedGrub { get; set; }
	
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

	protected override void OnGrubDamaged( Grub grub )
	{
		if ( !DamageQueue.Contains( grub ) )
			DamageQueue.Enqueue( grub );
	}

	protected override void OnGrubDied( Grub grub )
	{
		const float grubDeathTurnRemainder = 3f;
		
		grub.Owner.OnGrubDied( grub );
		
		// If the grub is the active grub, end the turn.
		if ( grub == ActivePlayer.ActiveGrub )
		{
			TimeUntilTurnOver = Math.Min( TimeUntilTurnOver, grubDeathTurnRemainder );
		}
	}

	protected override void OnEquipmentUsed( Equipment.Equipment _ )
	{
		TimeUntilTurnOver = Math.Min( TimeUntilTurnOver, GrubsConfig.MovementGracePeriod );
	}

	protected override bool IsGameStarted()
	{
		return State is FreeForAllState.Playing;
	}

	public override bool IsPlayerActive( Player player )
	{
		return ActivePlayer == player && !TurnIsChanging;
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
		
		var inv = player.Components.Get<Inventory>();
		inv.Player = player;
		inv.InitializeWeapons( GrubsConfig.InfiniteAmmo );
			
		PlayerQueue.Add( player );
	}

	private void StartTurnChange()
	{
		Log.Info( $"Starting turn change (finished: {ActivePlayer})." );
		
		ActivePlayer.OnTurnEnd();
		TimeSinceTurnChangeStarted = 0;
		TurnIsChanging = true;
	}

	private void UpdateTurnChange()
	{
		Log.Trace( $"Damage Queue Count: {DamageQueue.Count}" );
		
		if ( DamageQueue.Count != 0 || !TimeUntilNextDamagedGrub )
		{
			Log.Trace( $"Let's process the damage queue: {DamageQueue.Count != 0} || {!TimeUntilNextDamagedGrub}" );
			ProcessDamageQueue();
			return;
		}
		
		Log.Trace( $"No more damage queue. Moving on." );
		
		if ( TimeSinceTurnChangeStarted < MinimumTurnChangeDuration )
			return;
		
		TurnIsChanging = false;
		TimeUntilTurnOver = GrubsConfig.TurnDuration;
		RotateActivePlayer();
	}

	private void ProcessDamageQueue()
	{
		const float timeBetweenDamagedGrubs = 0.4f;

		if ( !TimeUntilNextDamagedGrub ) 
			return;
		
		ActiveDamagedGrub = DamageQueue.Dequeue();
		Log.Info( $"Set ActiveDamagedGrub to {ActiveDamagedGrub}" );
		TimeUntilNextDamagedGrub = timeBetweenDamagedGrubs;
		Log.Info( $"TimeUntilNextDamagedGrub: {TimeUntilNextDamagedGrub}" );
		ActiveDamagedGrub.Health.ApplyDamage();
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
