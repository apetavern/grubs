﻿using Grubs.Common;
using Grubs.Drops;
using Grubs.Helpers;
using Grubs.Pawn;
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
	
	[Sync( SyncFlags.FromHost )]
	public bool SuddenDeathEnabled { get; private set; }

	private Queue<Grub> DamageQueue { get; } = new();
	private Queue<Grub> DeathQueue { get; } = new();
	private Grub ActiveDamagedGrub { get; set; }
	private TimeUntil TimeUntilNextDequeue { get; set; }
	
	private int RoundsUntilSuddenDeath { get; set; }
	
	/// <summary>
	/// The amount of time elapsed since we started changing turns.
	/// Used to check for the minimum and maximum turn change wait.
	/// </summary>
	private TimeSince TimeSinceTurnChangeStarted { get; set; }
	
	/// <summary>
	/// The amount of time elapsed since moving to GameOver state.
	/// </summary>
	private TimeSince TimeSinceGameOverStateStarted { get; set; }
	
	/// <summary>
	/// The amount of time elapsed since we tried resolving in turn change.
	/// </summary>
	private TimeSince TimeSinceResolveAttempted { get; set; }

	private const float MaximumWorldResolveDuration = 20f;

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
		
		GrubCount = GrubsConfig.GrubCount;
		RoundsUntilSuddenDeath = GrubsConfig.SuddenDeathDelay;

		// For each player, spawn Grubs and initialize their inventory.
		foreach ( var player in Players )
		{
			InitializePlayer( player );
		}

		RotateActivePlayer();
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

		if ( State is FreeForAllState.GameOver )
		{
			const float gameOverStateDuration = 12f;
			if ( TimeSinceGameOverStateStarted > gameOverStateDuration )
			{
				Log.Info( "GameOver state time elapsed. Moving to Lobby state." );
				State = FreeForAllState.Lobby;
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

	protected override void OnPlayerLeft( Player player )
	{
		if ( player == ActivePlayer )
		{
			Log.Info( "Active player has left, skipping turn!" );
			TimeUntilTurnOver = 0f;
		}

		Log.Info( "Giving all disconnected player's grubs disconnect damage." );
		foreach ( var grub in player.Grubs )
		{
			grub.Health.TakeDamage( GrubsDamageInfo.FromDisconnect() );
		}

		if ( State is not FreeForAllState.Playing )
		{
			player.GameObject.Destroy();
		}
	}

	protected override void OnGrubDamaged( Grub grub )
	{
		if ( !DamageQueue.Contains( grub ) )
			DamageQueue.Enqueue( grub );

		if ( IsGrubActive( grub ) )
			TimeUntilTurnOver = 0;
	}

	protected override void OnGrubDied( Grub grub )
	{
		const float grubDeathTurnRemainder = 3f;

		if ( !grub.IsValid() || !grub.Owner.IsValid() )
		{
			Log.Warning( "Tried to call OnGrubDied but Grub or Grub.Owner was invalid." );
			return;
		}
		
		grub.Owner.OnGrubDied( grub );
		
		// If the grub is the active grub, end the turn.
		if ( ActivePlayer.IsValid() && grub == ActivePlayer.ActiveGrub )
		{
			TimeUntilTurnOver = Math.Min( TimeUntilTurnOver, grubDeathTurnRemainder );
		}
		
		// If a grub died while the turn is changing, let's process their death immediately.
		if ( TurnIsChanging )
		{
			Log.Info( $"Grub died while turn is changing: {grub}." );
			DeathQueue.Enqueue( grub );
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
		if ( !ActivePlayer.IsValid() )
			return false;
		return ActivePlayer.ActiveGrub == grub && !TurnIsChanging;
	}

	private void InitializePlayer( Player player )
	{
		const float grubSize = 8f;
		
		if ( !player.IsValid() )
			return;

		player.IsPlaying = true;
		
		for ( var i = 0; i < GrubsConfig.GrubCount; i++ )
		{
			var spawnLocation = GrubsTerrain.Instance.FindSpawnLocation( size: grubSize );
			player.AddGrub( spawnLocation );
		}
		
		var inv = player.Components.Get<Inventory>();
		inv.Player = player;
		inv.InitializeWeapons();
			
		PlayerQueue.Add( player );
	}

	private void StartTurnChange()
	{
		Log.Info( $"Starting turn change (finished: {ActivePlayer})." );
		
		if ( ActivePlayer.IsValid() )
			ActivePlayer.OnTurnEnd();
		TimeSinceTurnChangeStarted = 0;
		TurnIsChanging = true;
	}

	private bool CratesResolved { get; set; } = false;
	private bool CratesSpawnStarted { get; set; } = false;
	
	private bool SuddenDeathEffectStarted { get; set; } = false;
	private bool SuddenDeathEffectEnded { get; set; } = false;

	private void UpdateTurnChange()
	{
		// Wait one second before trying to process the turn change, in case of any race conditions.
		if ( TimeSinceTurnChangeStarted < 1f )
			return;

		if ( !Resolution.IsWorldResolved() && TimeSinceTurnChangeStarted < MaximumWorldResolveDuration )
		{
			TimeSinceResolveAttempted = 0f;
			return;
		}
		
		// Wait 1s after resolving to avoid whiplash.
		if ( TimeSinceResolveAttempted < 1f )
			return;
		
		Log.Trace( $"Damage Queue Count: {DamageQueue.Count}" );

		if ( !TimeUntilNextDequeue )
			return;

		if ( DeathQueue.Count != 0 )
		{
			Log.Trace( $"Let's process the death queue (count: {DeathQueue.Count}" );
			ProcessDeathQueue();
			return;
		}
		
		if ( DamageQueue.Count != 0 )
		{
			Log.Trace( $"Let's process the damage queue: {DamageQueue.Count != 0} || {!TimeUntilNextDequeue}" );
			ProcessDamageQueue();
			return;
		}
		
		Log.Trace( $"No more damage queue. Moving on." );
		
		if ( !CratesSpawnStarted )
			_ = ResolveCrates();

		if ( !CratesResolved )
			return;

		if ( GrubsConfig.SuddenDeathEnabled )
		{
			SuddenDeathEnabled = RoundsUntilSuddenDeath <= 0;

			if ( !SuddenDeathEffectStarted && SuddenDeathEnabled )
				_ = HandleSuddenDeath();

			if ( !SuddenDeathEffectEnded && SuddenDeathEnabled )
				return;
		}

		var livingPlayers = Player.AllLiving.ToList();
		
		// If only one player or less is alive, the game is over.
		if ( livingPlayers.Count <= 1 && (!GrubsConfig.KeepGameAlive || Player.All.Sum( p => p.Grubs.Count ) < 1 ) )
		{
			Log.Info( "All players are dead. Moving to GameOver state." );
			State = FreeForAllState.GameOver;
			TimeSinceGameOverStateStarted = 0f;

			var winner = livingPlayers.FirstOrDefault();
			if ( winner != null )
			{
				ChatHelper.Instance.SendInfoMessage( $"{winner.Network.Owner.DisplayName} won the match!" );
			}
			
			ResetGameMode();
			return;
		}
		
		TurnIsChanging = false;
		TimeUntilTurnOver = GrubsConfig.TurnDuration;

		var playersToRemove = PlayerQueue.Where( player => !player.IsValid() || player.IsDisconnected ).ToList();
		foreach ( var player in playersToRemove )
		{
			PlayerQueue.Remove( player );
		}
		
		RotateActivePlayer();
		
		while ( ActivePlayer.IsDead && _rotateCount < Player.All.Count() )
			RotateActivePlayer();

		foreach ( var player in Player.All )
		{
			if ( player.IsDead && player.IsDisconnected )
			{
				player.GameObject.Destroy();
			}
		}
		
		// Send the ActivePlayer's ActiveGrub in case the synced ActiveGrub hasn't been processed yet.
		ActivePlayer.OnTurnStart( ActivePlayer.ActiveGrub );
		_rotateCount = 0;

		CratesSpawnStarted = false;
		CratesResolved = false;
		
		SuddenDeathEffectStarted = false;
		SuddenDeathEffectEnded = false;
	}

	private async Task ResolveCrates()
	{
		CratesSpawnStarted = true;
		
		await RollCrateSpawn( DropType.Weapon, GrubsConfig.WeaponCrateChancePerTurn,
			"A weapon crate has been spawned!" );
		await RollCrateSpawn( DropType.Health, GrubsConfig.HealthCrateChancePerTurn,
			"A health crate has been spawned!" );
		await RollCrateSpawn( DropType.Tool, GrubsConfig.ToolCrateChancePerTurn, "A tool crate has been spawned!" );

		CratesResolved = true;
	}

	private async Task RollCrateSpawn( DropType dropType, float chance, string message )
	{
		const float crateSpawnDelay = 2f;
		
		if ( Game.Random.Float( 1f ) >= chance )
			return;

		await Task.MainThread();

		var spawnPos = GrubsTerrain.Instance.FindSpawnLocation( inAir: true, maxAngle: 25f );
		var crate = CrateUtility.Instance.SpawnCrate( dropType );
		crate.WorldPosition = spawnPos;
		
		ChatHelper.Instance.SendInfoMessage( message );
		
		if ( GrubFollowCamera.Local.IsValid() )
			GrubFollowCamera.Local.QueueTarget( crate, crateSpawnDelay );

		await Task.DelaySeconds( crateSpawnDelay );
	}

	private async Task HandleSuddenDeath()
	{
		SuddenDeathEffectStarted = true;
		
		const float suddenDeathDelay = 1f;

		await GrubsTerrain.Instance.LowerTerrain( (float)GrubsConfig.SuddenDeathAggression );
		
		await Task.DelaySeconds( suddenDeathDelay );
		
		SuddenDeathEffectEnded = true;
	}

	private void ProcessDeathQueue()
	{
		const float timeBetweenDeaths = 2f;

		if ( !TimeUntilNextDequeue )
			return;
		
		var dyingGrub = DeathQueue.Dequeue();
		if ( !dyingGrub.IsValid() )
		{
			Log.Warning( $"Grub in death queue is invalid, skipping." );
			return;
		}

		ActiveDamagedGrub = dyingGrub;
		
		Log.Info( $"Set ActiveDamagedGrub to {ActiveDamagedGrub} from death queue." );
		
		QueueCameraTarget( ActiveDamagedGrub.GameObject, timeBetweenDeaths );
		TimeUntilNextDequeue = timeBetweenDeaths;
	}

	private void ProcessDamageQueue()
	{
		const float timeBetweenDamagedGrubs = 2f;

		if ( !TimeUntilNextDequeue ) 
			return;
		
		var damagedGrub = DamageQueue.Dequeue();
		if ( !damagedGrub.IsValid() )
		{
			Log.Warning( "Grub in queue is invalid, skipping." );
			return;
		}

		ActiveDamagedGrub = damagedGrub;
		
		Log.Info( $"Set ActiveDamagedGrub to {ActiveDamagedGrub} from damage queue." );
		
		QueueCameraTarget( ActiveDamagedGrub.GameObject, timeBetweenDamagedGrubs );
		TimeUntilNextDequeue = timeBetweenDamagedGrubs;
		ActiveDamagedGrub.Health.ApplyDamage();
	}

	private int _rotateCount;

	private void RotateActivePlayer()
	{
		Log.Info( $"Rotating active player (current: {ActivePlayer}) (count: {_rotateCount})." );
		if ( PlayerQueue.Count == 0 )
		{
			Log.Info( $"Lowering rounds until sudden death to {RoundsUntilSuddenDeath - 1}" );
			RoundsUntilSuddenDeath -= 1;
			
			foreach ( var player in Player.AllLiving )
				PlayerQueue.Add( player );
		}
		
		var nextPlayer = PlayerQueue.First();
		PlayerQueue.Remove( nextPlayer );
		
		Log.Info( $"New active player: {nextPlayer}." );
		ActivePlayer = nextPlayer;
		ActivePlayer?.RotateActiveGrub();
		
		_rotateCount++;
	}

	public void ResetGameMode()
	{
		Log.Info( "Resetting FFA mode to defaults." );

		State = FreeForAllState.GameOver;
		
		PlayerQueue.Clear();
		ActivePlayer = null;
		TurnIsChanging = false;
		SuddenDeathEnabled = false;
		
		DamageQueue.Clear();
		ActiveDamagedGrub = null;

		foreach ( var player in Player.All )
		{
			player.Cleanup();
		}
		
		Scene.Children
			.Where( x => x.Tags.HasAny( "projectile", "cleanup" ) )
			.ToList()
			.ForEach( x =>
			{
				Log.Info( $"Destroying {x}." );
				x.Root.Destroy();
			} );
		
		_rotateCount = 0;

		GrubsConfig.InfiniteAmmo = false;
		GrubsConfig.KeepGameAlive = false;
		IsSandboxMode = false;
		
		GrubsTerrain.Instance.Init();
	}

	[Rpc.Broadcast( NetFlags.HostOnly )]
	private void QueueCameraTarget( GameObject target, float duration )
	{
		if ( !target.IsValid() )
			return;

		GrubFollowCamera.Local?.QueueTarget( target, duration );
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
