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

	public Queue<Player> PlayerTurnQueue { get; set; } = new();

	private Task _nextTurnTask = null;

	internal override async void Initialize()
	{
		State = GameState.Menu;

		if ( Connection.Local == Connection.Host )
		{
			GrubsTerrain.Instance.Network.TakeOwnership();
			GrubsTerrain.Instance.Init();
		}
	}

	internal override void Start()
	{
		var players = Scene.GetAllComponents<Player>();
		foreach ( var player in players )
		{
			for ( var i = 0; i < GrubsConfig.GrubCount; i++ )
			{
				var go = player.GrubPrefab.Clone();
				go.Network.SetOrphanedMode( NetworkOrphaned.Host );
				go.NetworkSpawn();

				var grub = go.Components.Get<Grub>();
				SetGrubPlayer( player.Id, grub.Id );

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
		await GrubsTerrain.UntilResolve( 30 );
		RotateActivePlayer();

		_nextTurnTask = null;
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
		TimeUntilNextTurn = GrubsConfig.TurnDuration;
	}

	[Broadcast]
	public void SetGrubPlayer( Guid playerId, Guid grubId )
	{
		var player = playerId.ToComponent<Player>();
		var grub = grubId.ToComponent<Grub>();

		if ( player is null || grub is null )
			return;

		grub.Player = player;
	}

	[Broadcast]
	public void SetActiveGrub( Guid playerId, Guid grubId )
	{
		var player = playerId.ToComponent<Player>();
		var grub = grubId.ToComponent<Grub>();

		if ( player is null || grub is null )
			return;

		player.ActiveGrub = grub;
	}
}
