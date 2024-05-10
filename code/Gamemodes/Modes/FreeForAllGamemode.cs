using Grubs.Common;
using Grubs.Extensions;
using Grubs.Pawn;
using Grubs.Terrain;

namespace Grubs.Gamemodes.Modes;

[Title( "Grubs - FFA" ), Category( "Grubs" )]
public sealed class FreeForAllGamemode : Gamemode
{
	public override string GamemodeName => "Free For All";

	[Property, ReadOnly, HostSync] public Guid ActivePlayerId { get; set; }
	[HostSync] public TimeUntil TimeUntilNextTurn { get; set; }

	private Queue<Player> PlayerTurnQueue { get; set; } = new();
	private Dictionary<Player, Queue<Grub>> PlayerGrubOrder { get; set; } = new();

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
		var players = Scene.GetAllComponents<Player>();
		foreach ( var player in players )
		{
			PlayerGrubOrder.Add( player, new Queue<Grub>() );

			for ( var i = 0; i < GrubsConfig.GrubCount; i++ )
			{
				var go = player.GrubPrefab.Clone();
				go.Network.SetOrphanedMode( NetworkOrphaned.Host );
				go.NetworkSpawn();

				var grub = go.Components.Get<Grub>();
				SetGrubPlayer( player.Id, grub.Id );

				var queue = PlayerGrubOrder[player];
				queue.Enqueue( grub );

				go.Network.AssignOwnership( player.Network.OwnerConnection );

				if ( i == 0 )
				{
					SetActiveGrub( player.Id, grub.Id );
				}
			}

			var inv = player.Components.Get<PlayerInventory>();
			inv.Player = player;
			inv.InitializeWeapons();

			PlayerTurnQueue.Enqueue( player );
		}

		var firstPlayer = PlayerTurnQueue.Dequeue();
		var firstGrub = PlayerGrubOrder[firstPlayer].Dequeue();
		PlayerGrubOrder[firstPlayer].Enqueue( firstGrub );
		ActivePlayerId = firstPlayer.Id;
		Started = true;
		State = GameState.Playing;
		TimeUntilNextTurn = GrubsConfig.TurnDuration;
	}

	protected override void OnUpdate()
	{
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

	private async Task NextTurn()
	{
		TurnIsChanging = true;

		var player = ActivePlayerId.ToComponent<Player>();
		player?.EndTurn();

		await Resolution.UntilWorldResolved( 30 );

		await GameTask.Delay( 1000 );
		await ApplyDamageQueue();

		RotateActivePlayer();

		_nextTurnTask = null;

		TurnIsChanging = false;
	}

	private void RotateActivePlayer()
	{
		if ( !PlayerTurnQueue.Any() )
		{
			foreach ( var player in Scene.GetAllComponents<Player>() )
			{
				if ( !player.ShouldHaveTurn )
					continue;
				PlayerTurnQueue.Enqueue( player );
			}
		}

		var nextPlayer = PlayerTurnQueue.Dequeue();
		ActivePlayerId = nextPlayer.Id;

		var nextGrub = FindNextGrub( nextPlayer );
		nextPlayer.ActiveGrubId = nextGrub.Id;
		SetActiveGrub( nextPlayer.Id, nextGrub.Id );

		TimeUntilNextTurn = GrubsConfig.TurnDuration;
	}

	private Grub FindNextGrub( Player player )
	{
		var queue = PlayerGrubOrder[player];
		while ( queue.Any() )
		{
			var grub = queue.Dequeue();
			if ( !grub.IsValid() || grub.IsDead )
				continue;
			queue.Enqueue( grub );
			return grub;
		}

		return null;
	}

	[Broadcast]
	public void SetGrubPlayer( Guid playerId, Guid grubId )
	{
		var player = playerId.ToComponent<Player>();
		var grub = grubId.ToComponent<Grub>();

		if ( player is null || grub is null )
			return;

		grub.Player = player;
		Log.Info( $"Adding grub {grub.Id} to player {player.Id} at index {player.Grubs.Count}" );
		player.Grubs.Add( grub.Id );
	}

	[Broadcast]
	public void SetActiveGrub( Guid playerId, Guid grubId )
	{
		var player = playerId.ToComponent<Player>();
		var grub = grubId.ToComponent<Grub>();

		Log.Info( $"Setting active grub to {grubId} for player {playerId}" );

		if ( player is null || grub is null )
			return;

		player.ActiveGrub = grub;
	}
}
