﻿namespace Grubs;

public partial class FreeForAll : Gamemode
{
	public override string GamemodeName => "Free For All";
	public override bool AllowFriendlyFire => true;
	public override int MinimumPlayers => GrubsConfig.MinimumPlayers;

	public enum GameState
	{
		Waiting,
		Playing,
		GameOver
	}

	public override string GetGameStateLabel()
	{
		return CurrentState switch
		{
			GameState.Waiting => "Waiting for game to begin",
			GameState.Playing => "Game in progress",
			GameState.GameOver => "Game is over",
			_ => null,
		};
	}

	[Net]
	public GameState CurrentState { get; set; }

	/// <summary>
	/// Whether or not the GameWorld modifications have finished transmitting to clients.
	/// TODO: What if a new player joins?
	/// </summary>
	[Net]
	public bool TerrainReady { get; set; } = false;

	/// <summary>
	/// The amount of time before the current player's turn is concluded.
	/// </summary>
	[Net]
	public TimeUntil TimeUntilNextTurn { get; set; }

	/// <summary>
	/// A queue of players determining their turn order.
	/// </summary>
	public Queue<Player> PlayerTurnQueue { get; set; } = new();

	/// <summary>
	/// Whether we have started the game or not.
	/// </summary>
	[Net] public bool Started { get; set; } = false;

	/// <summary>
	/// An async task for switching between player turns.
	/// </summary>
	public Task NextTurnTask { get; set; }

	public override float GetTimeRemaining()
	{
		return TimeUntilNextTurn;
	}

	internal override void Initialize()
	{
		base.Initialize();

		AllowDamage = false;
		CurrentState = GameState.Waiting;
	}

	internal override void Start()
	{
		SpawnPlayers();

		CurrentState = GameState.Playing;
		Started = true;
	}

	/// <summary>
	/// Spawn a Player and its Grubs for each client.
	/// Then, set the ActivePlayer.
	/// </summary>
	private void SpawnPlayers()
	{
		foreach ( var client in Game.Clients )
		{
			if ( client.Pawn is not Player player )
				continue;

			player.Respawn();
			Players.Add( player );
			PlayerTurnQueue.Enqueue( player );

			MoveToSpawnpoint( client );
		}

		ActivePlayer = PlayerTurnQueue.Dequeue();
	}

	internal override void UseTurn( bool giveMovementGrace = false )
	{
		if ( giveMovementGrace )
		{
			TimeUntilNextTurn = GrubsConfig.MovementGracePeriod;
		}
		else
		{
			UsedTurn = true;
		}
	}

	private async Task NextTurn()
	{
		ActivePlayer.EndTurn();

		await World.UntilResolve();

		TurnIsChanging = true;

		if ( await CleanupTurn() )
			return;

		if ( GrubsConfig.WindEnabled )
			ActiveWindSteps = Game.Random.Int( -GrubsConfig.WindSteps, GrubsConfig.WindSteps );

		RotateActivePlayer();

		UsedTurn = false;
		TimeUntilNextTurn = GrubsConfig.TurnDuration;
		TurnIsChanging = false;
		NextTurnTask = null;

		await SetupTurn();
	}

	/// <summary>
	/// Handle cleaning up the existing player's turn.
	/// </summary>
	private async ValueTask<bool> CleanupTurn()
	{
		await GameTask.DelaySeconds( 1f );
		await HandleGrubDeaths();
		await HandleCrateSpawns();

		return CheckWinConditions();
	}

	private async Task HandleGrubDeaths()
	{
		foreach ( var player in Players )
		{
			if ( player.IsDead )
				continue;

			foreach ( var grub in player.Grubs )
			{
				if ( grub.LifeState == LifeState.Dead )
					continue;

				if ( player.IsDisconnected )
					grub.TakeDamage( DamageInfo.Generic( float.MaxValue ).WithTag( "disconnect" ) );

				if ( !grub.HasBeenDamaged )
					continue;

				await HandleGrubDeath( grub );

				await World.UntilResolve();
			}
		}
	}

	private async Task HandleGrubDeath( Grub grub )
	{
		if ( grub.ApplyDamage() && grub.DeathTask is not null && !grub.DeathTask.IsCompleted )
			await grub.DeathTask;

		if ( grub.Position.z < -GrubsConfig.TerrainHeight )
			return;

		CameraTarget = grub;

		DamageGrubEventClient( To.Everyone, grub );
		await GameTask.Delay( 1000 );

		CameraTarget = null;
	}

	private async Task HandleCrateSpawns()
	{
		await CheckCrateSpawn( CrateType.Weapons, GrubsConfig.WeaponCrateChancePerTurn, "A weapons crate has been spawned!" );
		await CheckCrateSpawn( CrateType.Tools, GrubsConfig.ToolCrateChancePerTurn, "A tool crate has been spawned!" );
		await CheckCrateSpawn( CrateType.Health, GrubsConfig.HealthCrateChancePerTurn, "A health crate has been spawned!" );

		await World.UntilResolve();

		CameraTarget = null;
	}

	private async Task CheckCrateSpawn( CrateType crateType, int chance, string message )
	{
		if ( Game.Random.Int( 100 ) > chance )
			return;

		var player = Game.Clients.First().Pawn as Player;
		var crate = CrateGadgetComponent.SpawnCrate( crateType );

		var spawnPos = GameWorld.FindSpawnLocation();
		crate.Position = spawnPos;
		crate.Owner = player;
		player.Gadgets.Add( crate );

		TextChat.AddInfoChatEntry( message );
		CameraTarget = crate;

		await GameTask.DelaySeconds( 2f );
	}

	[ClientRpc]
	public void DamageGrubEventClient( Grub grub )
	{
		Event.Run( GrubsEvent.Grub.Damaged, grub.NetworkIdent );
	}

	/// <summary>
	/// Handle setting up the new player's turn.
	/// </summary>
	private async Task SetupTurn()
	{
		// TODO: I am not sure.
	}

	private bool CheckWinConditions()
	{
		var deadPlayers = 0;
		Player lastPlayerAlive;

		foreach ( var player in Players )
		{
			if ( player.IsDead )
			{
				deadPlayers++;
				continue;
			}

			lastPlayerAlive = player;
		}

		// TODO: Pass win/lose/draw information.
		if ( deadPlayers == Players.Count )
		{
			// Draw
			CurrentState = GameState.GameOver;
			return true;
		}

		if ( deadPlayers == Players.Count - 1 )
		{
			// 1 Player remaining
			CurrentState = GameState.GameOver;
			return true;
		}

		return false;
	}

	private void RotateActivePlayer()
	{
		if ( ActivePlayer.IsAvailableForTurn )
			PlayerTurnQueue.Enqueue( ActivePlayer );

		ActivePlayer = PlayerTurnQueue.Dequeue();
		while ( !ActivePlayer.IsAvailableForTurn )
		{
			ActivePlayer = PlayerTurnQueue.Dequeue();
		}

		ActivePlayer.PickNextGrub();
	}

	internal override void MoveToSpawnpoint( IClient client )
	{
		if ( client.Pawn is not Player player )
			return;

		foreach ( var grub in player.Grubs )
		{
			var spawnPos = GameWorld.FindSpawnLocation();
			grub.Position = spawnPos;
		}
	}

	private bool CheckCurrentPlayerFiring()
	{
		if ( !ActivePlayer.ActiveGrub.IsValid() )
			return false;

		var weapon = ActivePlayer.ActiveGrub.ActiveWeapon;
		return weapon.IsValid() && weapon.IsFiring() && !weapon.AllowMovement;
	}

	[Event.Tick.Server]
	private void Tick()
	{
		//
		// Waiting Logic
		//
		if ( CurrentState is GameState.Waiting )
		{
			if ( !TerrainReady )
			{
				if ( GameWorld is null || GameWorld.CsgWorld is null )
					return;

				TerrainReady = GameWorld.CsgWorld.TimeSinceLastModification > 1f;
			}
		}
		//
		// Playing Logic
		//
		else if ( CurrentState is GameState.Playing )
		{
			if ( NextTurnTask is not null && !NextTurnTask.IsCompleted )
				return;

			if ( !ActivePlayer.ActiveGrub.IsValid() || ActivePlayer.IsDisconnected )
				UseTurn( false );

			if ( TimeUntilNextTurn <= 0f && !UsedTurn )
				UseTurn();

			if ( UsedTurn )
				NextTurnTask ??= NextTurn();

			AllowMovement = !CheckCurrentPlayerFiring();
		}
		//
		// Game Over Logic
		//
		else if ( CurrentState is GameState.GameOver )
		{

		}

		if ( Debug && CurrentState is GameState.Playing )
		{
			var lineOffset = 19;
			DebugOverlay.ScreenText( $"ActivePlayer & Grub: {ActivePlayer.Client.Name} - {ActivePlayer.ActiveGrub.Name}", lineOffset++ );
			DebugOverlay.ScreenText( $"TimeUntilNextTurn: {TimeUntilNextTurn}", lineOffset++ );
			DebugOverlay.ScreenText( $"UsedTurn: {UsedTurn}", lineOffset++ );
		}
	}

	[ConCmd.Admin( "gr_skip_turn" )]
	public static void SkipTurn()
	{
		if ( GamemodeSystem.Instance is not FreeForAll ffa )
			return;

		ffa.NextTurnTask = null;
		ffa.UseTurn();
	}

	[ConCmd.Admin( "gr_wind_steps" )]
	public static void SetWindSteps( int wind )
	{
		if ( GamemodeSystem.Instance is not FreeForAll ffa )
			return;

		ffa.ActiveWindSteps = wind;
	}

	[ConVar.Replicated( "gr_debug_ffa" )]
	public static bool Debug { get; set; } = false;
}
