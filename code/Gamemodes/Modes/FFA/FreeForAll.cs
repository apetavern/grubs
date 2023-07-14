namespace Grubs;

public partial class FreeForAll : Gamemode
{
	public override string GamemodeName => "Free For All";
	public override int MinimumPlayers => GrubsConfig.MinimumPlayers;

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
	/// An async task for switching between player turns.
	/// </summary>
	public Task NextTurnTask { get; set; }

	private Queue<Player> LateJoinSpawnQueue { get; set; } = new();
	private string[] _lateJoinPhrases = new string[3] { "arrived fashionably late!", "has dropped in unexpectedly!", "finally decided to showed up!" };

	public override float GetTimeRemaining()
	{
		return TimeUntilNextTurn;
	}

	internal override void Initialize()
	{
		base.Initialize();

		CurrentState = State.MainMenu;
	}

	internal override void Start()
	{
		Terrain.ResetTerrainPosition();

		SpawnPlayers();

		TimeUntilNextTurn = GrubsConfig.TurnDuration;
		CurrentState = State.Playing;
		base.Start();
	}

	internal override void IncrementGamesPlayedStat() => Stats.IncrementGamesPlayed( "ffa" );

	internal override void OnPlayerJoinedLate( Player player )
	{
		if ( !GrubsConfig.SpawnLateJoiners )
			return;

		if ( !LateJoinSpawnQueue.Contains( player ) || DisconnectedPlayers.Contains( player ) )
			LateJoinSpawnQueue.Enqueue( player );

		base.OnPlayerJoinedLate( player );
	}

	/// <summary>
	/// Spawn a Player and its Grubs for each client.
	/// Then, set the ActivePlayer.
	/// </summary>
	private void SpawnPlayers()
	{
		foreach ( var client in Game.Clients.Shuffle().OrderByDescending( x => !x.IsBot ) )
		{
			if ( client.Pawn is not Player player )
				continue;

			player.Respawn();
			Players.Add( player );
			PlayerTurnQueue.Enqueue( player );

			MoveToSpawnpoint( client );
		}

		RotateActivePlayer();
	}

	internal override void UseTurn( bool giveMovementGrace = false )
	{
		if ( giveMovementGrace )
			TimeUntilNextTurn = GrubsConfig.MovementGracePeriod;
		else
			UsedTurn = true;
	}

	internal override async Task OnRoundPassed()
	{
		await base.OnRoundPassed();
		await CheckSuddenDeath();
	}

	private async Task NextTurn()
	{
		TurnIsChanging = true;

		ActivePlayer.EndTurn();

		await Terrain.UntilResolve( 30 );

		await CleanupTurn();

		if ( !PlayerTurnQueue.Any() )
			await OnRoundPassed();

		if ( HasGameWinner() )
		{
			Event.Run( GrubsEvent.Game.End );
			return;
		}
		else if ( Game.Clients.Where( C => C.IsBot ).Any() )
		{
			var worldbox = new BBox
			{
				Maxs = new Vector3( Terrain.WorldTextureLength / 2f, 10f, Terrain.WorldTextureHeight ),
				Mins = new Vector3( -Terrain.WorldTextureLength / 2f, -10f, 0 )
			};

			await GridAStar.Grid.Create( Vector3.Zero, worldbox, Rotation.Identity, worldOnly: false, heightClearance: 30f, stepSize: 50f, standableAngle: 50f, save: false );
		}

		if ( GrubsConfig.WindEnabled )
			ActiveWindSteps = Game.Random.Int( -GrubsConfig.WindSteps, GrubsConfig.WindSteps );

		await HandleSpawns();

		RotateActivePlayer();

		UsedTurn = false;
		TimeUntilNextTurn = GrubsConfig.TurnDuration;
		TurnIsChanging = false;
		NextTurnTask = null;

		await SetupTurn();
	}

	private bool HasGameWinner() => CheckWinConditions();

	private async Task DealGrubDamage()
	{
		foreach ( var player in Players )
		{
			if ( player.IsDead || !player.IsDisconnected )
				continue;

			foreach ( var grub in player.Grubs )
			{
				if ( grub.LifeState == LifeState.Alive )
					grub.TakeDamage( DamageInfo.Generic( float.MaxValue ).WithTag( "disconnect" ) );
			}
		}

		while ( GamemodeSystem.Instance.DamageQueue.Any() )
		{
			var damagedGrub = GamemodeSystem.Instance.DamageQueue.Dequeue();
			if ( !damagedGrub.IsValid() )
				continue;

			while ( !damagedGrub.Resolved )
				await GameTask.Delay( 200 );

			var damageTaken = damagedGrub.ApplyDamage();
			if ( damageTaken <= 0 )
				continue;

			await ShowDamagedGrub( damagedGrub );
			await Terrain.UntilResolve( 30 );
		}

		// Clear dead or disconnected players from player turn queue.
		PlayerTurnQueue = new( PlayerTurnQueue.Where( p => p.IsAvailableForTurn ) );
	}

	private async Task ShowDamagedGrub( Grub grub )
	{
		if ( grub.DeathTask is not null && !grub.DeathTask.IsCompleted )
			await grub.DeathTask;

		if ( grub.Position.z < -GrubsConfig.TerrainHeight )
			return;

		CameraTarget = grub;

		DamageGrubEventClient( To.Everyone, grub );
		await GameTask.Delay( 1250 );

		CameraTarget = null;
	}

	private async Task HandleCrateSpawns()
	{
		await CheckCrateSpawn( CrateType.Weapons, GrubsConfig.WeaponCrateChancePerTurn, "A weapons crate has been spawned!" );
		await CheckCrateSpawn( CrateType.Tools, GrubsConfig.ToolCrateChancePerTurn, "A tool crate has been spawned!" );
		await CheckCrateSpawn( CrateType.Health, GrubsConfig.HealthCrateChancePerTurn, "A health crate has been spawned!" );

		await Terrain.UntilResolve( 30 );

		CameraTarget = null;
	}

	private async Task CheckCrateSpawn( CrateType crateType, int chance, string message )
	{
		if ( Game.Random.Int( 100 ) > chance )
			return;

		var player = Game.Clients.First().Pawn as Player;
		var crate = CrateGadgetComponent.SpawnCrate( crateType );

		var spawnPos = Terrain.FindSpawnLocation( traceDown: false, crate );
		crate.Position = spawnPos;
		crate.Owner = player;
		player.Gadgets.Add( crate );

		UI.TextChat.AddInfoChatEntry( message );
		CameraTarget = crate;

		await GameTask.DelaySeconds( 2 );
	}

	private async Task HandleBarrelSpawn()
	{
		if ( Game.Random.Int( 100 ) > GrubsConfig.BarrelChancePerTurn )
			return;

		PrefabLibrary.TrySpawn<Gadget>( "prefabs/world/oil_drum.prefab", out var barrel );
		var player = Game.Clients.First().Pawn as Player;

		var spawnPos = Terrain.FindSpawnLocation( traceDown: false, barrel );
		barrel.Position = spawnPos;
		barrel.Owner = player;
		player.Gadgets.Add( barrel );

		UI.TextChat.AddInfoChatEntry( "A dancing barrel just spawned... what the spruce??" );
		CameraTarget = barrel;

		await GameTask.DelaySeconds( 2 );
		CameraTarget = null;
	}

	private async Task HandleLateJoinerSpawn()
	{
		if ( LateJoinSpawnQueue.Count <= 0 )
			return;

		var player = LateJoinSpawnQueue.Dequeue();
		if ( !player.IsValid() )
			return;

		player.HandleLateJoin();
		Players.Add( player );
		PlayerTurnQueue.Enqueue( player );

		if ( !player.ActiveGrub.Components.TryGet<LateJoinMechanic>( out var lateJoin ) )
			return;

		UI.TextChat.AddInfoChatEntry( $"{player.Client.Name} {Game.Random.FromArray( _lateJoinPhrases )}" );

		while ( !lateJoin.FinishedParachuting || !player.ActiveGrub.Resolved )
		{
			CameraTarget = player.ActiveGrub;
			await GameTask.Delay( 1 );
		}

		CameraTarget = null;
	}

	[ClientRpc]
	public void DamageGrubEventClient( Grub grub )
	{
		Event.Run( GrubsEvent.Grub.Damaged, grub.NetworkIdent );
	}

	/// <summary>
	/// Handle cleaning up the existing player's turn.
	/// </summary>
	/// <returns></returns>
	private async Task CleanupTurn()
	{
		await GameTask.DelaySeconds( 1f );
		await DealGrubDamage();
	}

	/// <summary>
	/// Handle spawning of game elements.
	/// </summary>
	/// <returns></returns>
	private async Task HandleSpawns()
	{
		await HandleCrateSpawns();
		await HandleBarrelSpawn();
		await HandleLateJoinerSpawn();
	}

	/// <summary>
	/// Handle setting up the next player's turn.
	/// </summary>
	internal override async Task SetupTurn()
	{
		await base.SetupTurn();
	}

	private bool CheckWinConditions()
	{
		var deadPlayers = 0;
		Player lastPlayerAlive = null;

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
			CurrentState = State.GameOver;
			return true;
		}

		if ( deadPlayers == Players.Count - 1 )
		{
			// 1 Player remaining
			Stats.IncrementGamesWon( "ffa", lastPlayerAlive.Client );
			CurrentState = State.GameOver;
			return true;
		}

		return false;
	}

	public void RotateActivePlayer()
	{
		if ( !PlayerTurnQueue.Any() )
		{
			foreach ( var player in Players.Where( p => p.IsAvailableForTurn ) )
			{
				PlayerTurnQueue.Enqueue( player );
			}
		}

		ActivePlayer = PlayerTurnQueue.Dequeue();

		GameTask.Delay( 200 );

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
			var spawnPos = Terrain.FindSpawnLocation( size: 16f );
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

	[GameEvent.Tick.Server]
	private void Tick()
	{
		//
		// MainMenu Logic
		//
		if ( CurrentState is State.MainMenu )
		{
			if ( Terrain is null || Terrain.SdfWorld is null )
				return;
		}
		//
		// Playing Logic
		//
		else if ( CurrentState is State.Playing )
		{
			if ( NextTurnTask is not null && !NextTurnTask.IsCompleted )
				return;

#if DEBUG
			if ( GrubsConfig.InstantlyEndBotTurns && ActivePlayer.Client.IsBot )
				UseTurn( false );
#endif

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
		else if ( CurrentState is State.GameOver )
		{

		}

		if ( Debug && CurrentState is State.Playing )
		{
			var lineOffset = 19;
			DebugOverlay.ScreenText( $"ActivePlayer & Grub: {ActivePlayer.Client.Name} - {ActivePlayer.ActiveGrub.Name}", lineOffset++ );
			DebugOverlay.ScreenText( $"TimeUntilNextTurn: {TimeUntilNextTurn}", lineOffset++ );
			DebugOverlay.ScreenText( $"UsedTurn: {UsedTurn}", lineOffset++ );
			DebugOverlay.ScreenText( $"RoundsPassed: {RoundsPassed}", lineOffset++ );
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
