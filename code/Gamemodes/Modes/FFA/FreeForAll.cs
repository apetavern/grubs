namespace Grubs;

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
	/// A list of players, rotated cyclically to rotate through turns.
	/// </summary>
	public List<Player> PlayerRotation { get; set; } = new();

	/// <summary>
	/// Whether we have started the game or not.
	/// </summary>
	public bool Started { get; set; } = false;

	/// <summary>
	/// An async task for switching between player turns.
	/// </summary>
	public Task NextTurnTask { get; set; }

	internal override void Initialize()
	{
		base.Initialize();

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
			var player = new Player();
			client.Pawn = player;
			PlayerRotation.Add( player );

			MoveToSpawnpoint( client );
		}


		ActivePlayer = PlayerRotation[0];
	}

	private void ZoneTrigger()
	{
		var grubs = All.OfType<Grub>();
		foreach ( var grub in grubs )
		{
			foreach ( var zone in TerrainZone.All.OfType<DamageZone>() )
			{
				if ( !zone.IsValid || !zone.InstantKill || !zone.InZone( grub ) )
					continue;

				zone.Trigger( grub );
			}
		}
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
		TurnIsChanging = true;
		await CleanupTurn();

		// TODO: Check win condition.

		RotateActivePlayer();
		UsedTurn = false;
		TimeUntilNextTurn = GrubsConfig.TurnDuration;
		TurnIsChanging = false;

		await SetupTurn();
	}

	/// <summary>
	/// Handle cleaning up the existing player's turn.
	/// </summary>
	private async Task CleanupTurn()
	{
		ActivePlayer.EndTurn();
		await GameTask.DelaySeconds( 1f );

		// TODO: Handle Grub deaths.
		bool rerun;
		do
		{
			rerun = false;
			foreach ( var grub in All.OfType<Grub>() )
			{
				if ( grub.LifeState == LifeState.Dead )
					continue;

				if ( !grub.HasBeenDamaged )
					continue;

				rerun = true;
				if ( grub.ApplyDamage() && grub.DeathTask is not null && !grub.DeathTask.IsCompleted )
					await grub.DeathTask;

				await GameTask.Delay( 300 );
			}
		} while ( rerun );

		// TODO: Handle potential crate spawns.
	}

	/// <summary>
	/// Handle setting up the new player's turn.
	/// </summary>
	private async Task SetupTurn()
	{
		// TODO: I am not sure.
	}

	private void RotateActivePlayer()
	{
		var current = ActivePlayer;
		ActivePlayer.RotateGrubs();

		PlayerRotation.RemoveAt( 0 );
		PlayerRotation.Add( current );

		ActivePlayer = PlayerRotation[0];
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
				TerrainReady = GameWorld.CsgWorld.TimeSinceLastModification > 1f;
			}

			if ( PlayerCount >= MinimumPlayers && TerrainReady && !Started )
			{
				Start();
			}
		}

		//
		// Playing Logic
		//
		if ( CurrentState is GameState.Playing )
		{
			if ( TimeUntilNextTurn <= 0f && !UsedTurn )
			{
				UseTurn();
			}

			if ( UsedTurn && (NextTurnTask is null || NextTurnTask.IsCompleted) )
			{
				NextTurnTask = NextTurn();
			}

			ZoneTrigger();
			CameraTarget = ActivePlayer.ActiveGrub;
		}

		//
		// Game Over Logic
		//
		if ( CurrentState is GameState.GameOver )
		{

		}

		if ( Debug && CurrentState is GameState.Playing )
		{
			var lineOffset = 19;
			DebugOverlay.ScreenText( $"ActivePlayer & Grub: {ActivePlayer.Client.Name} - {ActivePlayer.ActiveGrub.Name}", lineOffset++ );
			DebugOverlay.ScreenText( $"TimeUntilNextTurn: {TimeUntilNextTurn}", lineOffset++ );
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

	[ConVar.Replicated( "gr_debug_ffa" )]
	public static bool Debug { get; set; } = false;
}
