using Grubs.Common;
using Grubs.Drops;
using Grubs.Equipment.Gadgets.Ground;
using Grubs.Extensions;
using Grubs.Helpers;
using Grubs.Pawn;
using Grubs.Terrain;
using Grubs.UI.GameEnd;

namespace Grubs.Gamemodes.Modes;

[Title( "Grubs - FFA" ), Category( "Grubs" )]
public sealed class FreeForAllGamemode : Gamemode, Component.INetworkListener
{
	public override string GamemodeName => "Free For All";
	public override string GamemodeShortName => "ffa";

	[Property, ReadOnly, HostSync] public Guid ActivePlayerId { get; set; }
	public TimeUntil TimeUntilNextTurn { get; set; }

	private Task _nextTurnTask = null;

	internal override async void Initialize()
	{
		State = GameState.Menu;

		if ( Networking.IsHost )
		{
			GrubsTerrain.Instance.Init();
		}
	}

	internal override void Start()
	{
		base.Start();

		var players = Scene.GetAllComponents<Player>();
		foreach ( var player in players )
		{
			player.GrubQueue = new NetList<Guid>();

			for ( var i = 0; i < GrubsConfig.GrubCount; i++ )
			{
				var go = player.GrubPrefab.Clone();
				go.Network.SetOrphanedMode( NetworkOrphaned.Host );
				go.NetworkSpawn();

				var grub = go.Components.Get<Grub>();
				SetGrubPlayer( player.Id, grub.Id );

				player.GrubQueue.Add( grub.Id );

				go.Network.AssignOwnership( player.Network.OwnerConnection );

				if ( i == 0 )
				{
					SetActiveGrub( player.Id, grub.Id );
				}
			}

			var inv = player.Components.Get<PlayerInventory>();
			inv.Player = player;
			inv.InitializeWeapons( GrubsConfig.InfiniteAmmo );

			PlayerTurnQueue.Add( player.Id );
		}

		var firstPlayer = PlayerTurnQueue[0].ToComponent<Player>();
		PlayerTurnQueue.RemoveAt( 0 );
		var firstGrubId = firstPlayer.GrubQueue[0];
		firstPlayer.GrubQueue.RemoveAt( 0 );
		firstPlayer.GrubQueue.Add( firstGrubId );
		firstPlayer.OnTurn();
		ActivePlayerId = firstPlayer.Id;

		// Landmine Spawning
		for ( var i = 0; i < GrubsConfig.LandmineSpawnCount; i++ )
		{
			var spawnPos = GrubsTerrain.Instance.FindSpawnLocation( inAir: false, maxAngle: 25f );
			LandmineUtility.Instance.Spawn( spawnPos );
		}

		Started = true;
		State = GameState.Playing;
		SetTimeUntilNextTurn( GrubsConfig.TurnDuration );
	}

	protected override void OnUpdate()
	{
		if ( IsProxy )
			return;

		if ( State is GameState.Playing )
		{
			UpdatePlaying();
		}
	}

	private void UpdatePlaying()
	{
		if ( _nextTurnTask is not null && !_nextTurnTask.IsCompleted )
			return;

		var nextTurn = TimeUntilNextTurn;
		if ( nextTurn )
		{
			_nextTurnTask ??= NextTurn();
		}
	}

	[Broadcast( NetPermission.HostOnly )]
	public void SetTimeUntilNextTurn( float time ) => TimeUntilNextTurn = time;

	[Authority]
	public void UseTurn( bool giveMovementGrace = false )
	{
		if ( TurnIsChanging )
			return;

		if ( giveMovementGrace )
			SetTimeUntilNextTurn( GrubsConfig.MovementGracePeriod );
		else
			_nextTurnTask ??= NextTurn();
	}

	private async Task NextTurn()
	{
		TurnIsChanging = true;
		Resolution.ClearForceResolved( false );

		EndTurn();

		await Resolution.UntilWorldResolved( 30 );

		await GameTask.DelayRealtime( 1000 );
		await ApplyDamageQueue();

		if ( !PlayerTurnQueue.Any() )
			await OnRoundPassed();

		if ( IsGameResolved() && GrubsConfig.KeepGameAlive != true )
		{
			var winner = Scene.GetAllComponents<Player>().FirstOrDefault( p => !p.IsDead() );
			if ( winner is not null )
			{
				using ( Rpc.FilterInclude( winner.Network.OwnerConnection ) )
				{
					Stats.IncrementGamesWon( GamemodeShortName );
				}
			}

			// GameEnd.Instance.ShouldShow = true;
			State = GameState.GameOver;
			await Cleanup();
			return;
		}

		await HandleSpawns();

		RotateActivePlayer();

		_nextTurnTask = null;

		TurnIsChanging = false;
	}

	private async Task Cleanup()
	{
		foreach ( var player in Player.All )
		{
			player.Cleanup();
			player.GrubQueue.Clear();
		}

		PlayerTurnQueue.Clear();
		_nextTurnTask = null;
		TurnIsChanging = false;
		ActivePlayerId = Guid.Empty;
		Scene.Children.OfType<GameObject>().Where( x => x.Tags.HasAny( "projectile", "cleanup" ) ).ToList().ForEach( x => x.Destroy() );

		Started = false;

		GrubsTerrain.Instance.Init();
	}

	public void OnDisconnected( Connection connection )
	{
		var player = Scene.GetAllComponents<Player>().FirstOrDefault( p => p.Network.OwnerConnection == connection );
		if ( player?.IsActive ?? false )
		{
			UseTurn();
		}
	}

	private async Task HandleSpawns()
	{
		await HandleCrateSpawns();
	}

	private async Task HandleCrateSpawns()
	{
		await RollCrateSpawn( DropType.Weapon, GrubsConfig.WeaponCrateChancePerTurn,
			"A weapon crate has been spawned!" );
		await RollCrateSpawn( DropType.Health, GrubsConfig.HealthCrateChancePerTurn,
			"A health crate has been spawned!" );
		await RollCrateSpawn( DropType.Tool, GrubsConfig.ToolCrateChancePerTurn, "A tool crate has been spawned!" );
	}

	private async Task RollCrateSpawn( DropType dropType, float chance, string message )
	{
		if ( Game.Random.Float( 1f ) >= chance )
			return;

		var spawnPos = GrubsTerrain.Instance.FindSpawnLocation( inAir: true, maxAngle: 25f );
		var crate = CrateUtility.Instance.SpawnCrate( dropType );
		crate.Transform.Position = spawnPos;

		ChatHelper.Instance.SendInfoMessage( message );
	}

	private void RotateActivePlayer()
	{
		if ( !PlayerTurnQueue.Any( p => p.ToComponent<Player>()?.ShouldHaveTurn ?? false ) )
		{
			PlayerTurnQueue.Clear();
			foreach ( var player in Player.All )
			{
				if ( player is null || !player.ShouldHaveTurn )
					continue;
				PlayerTurnQueue.Add( player.Id );
			}
		}

		var nextPlayer = PlayerTurnQueue[0].ToComponent<Player>();
		PlayerTurnQueue.RemoveAt( 0 );

		while ( nextPlayer is null || !nextPlayer.ShouldHaveTurn )
		{
			nextPlayer = PlayerTurnQueue[0].ToComponent<Player>();
			PlayerTurnQueue.RemoveAt( 0 );
		}

		ActivePlayerId = nextPlayer.Id;

		var nextGrub = FindNextGrub( nextPlayer );
		SetActiveGrub( nextPlayer.Id, nextGrub.Id );
		nextPlayer.OnTurn();


		SetTimeUntilNextTurn( GrubsConfig.TurnDuration );
	}

	private Grub FindNextGrub( Player player )
	{
		var queue = player.GrubQueue;
		while ( queue.Any() )
		{
			var grubId = queue[0];
			var grub = grubId.ToComponent<Grub>();
			queue.RemoveAt( 0 );
			queue.Add( grubId );
			if ( !grub.IsValid() || grub.IsDead )
				continue;
			return grub;
		}

		return null;
	}

	private bool IsGameResolved()
	{
		var deadPlayers = 0;
		Player lastPlayerAlive = null;

		var players = Scene.GetAllComponents<Player>();
		foreach ( var player in players )
		{
			if ( player.IsDead() )
			{
				deadPlayers++;
				continue;
			}

			lastPlayerAlive = player;
		}

		if ( players.Count() == deadPlayers )
		{
			// Draw
			Log.Info( "draw" );
			return true;
		}

		if ( players.Count() - 1 == deadPlayers )
		{
			GameEnd.Instance.Winner = lastPlayerAlive!.Network.OwnerConnection.DisplayName;
			return true;
		}

		return false;
	}

	[Broadcast]
	public void SetGrubPlayer( Guid playerId, Guid grubId )
	{
		var player = playerId.ToComponent<Player>();
		var grub = grubId.ToComponent<Grub>();

		if ( player is null || grub is null )
			return;

		grub.PlayerId = playerId;
		player.Grubs.Add( grub.Id );
	}

	[Broadcast]
	public void SetActiveGrub( Guid playerId, Guid grubId )
	{
		var player = playerId.ToComponent<Player>();
		if ( player is null )
			return;

		player.ActiveGrubId = grubId;
	}

	[Broadcast]
	public void EndTurn()
	{
		var player = ActivePlayerId.ToComponent<Player>();
		player?.EndTurn();
	}
}
