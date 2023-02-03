namespace Grubs;

public partial class FreeForAll : Gamemode
{
	public override string GamemodeName => "Free For All";
	public override bool AllowFriendlyFire => true;
	public override int MinimumPlayers => GrubsConfig.MinimumPlayers;

	[Net]
	public TimeUntil TimeUntilTurnOver { get; set; }

	public List<Player> PlayerRotation { get; set; } = new();

	private bool _gameHasStarted = false;

	internal override void Initialize()
	{

	}

	private void Start()
	{
		_gameHasStarted = true;

		foreach ( var client in Game.Clients )
		{
			var player = new Player();
			client.Pawn = player;
			PlayerRotation.Add( player );

			MoveToSpawnpoint( client );
		}

		ActivePlayer = PlayerRotation.First();
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
			var spawnPos = GrubsGame.Instance.World.FindSpawnLocation();
			grub.Position = spawnPos;
		}
	}

	[Event.Tick.Server]
	public void Tick()
	{
		if ( PlayerCount >= MinimumPlayers && !_gameHasStarted )
		{
			Start();
		}

		if ( _gameHasStarted && TimeUntilTurnOver < 0f )
		{
			RotateActivePlayer();
			TimeUntilTurnOver = GrubsConfig.TurnDuration;
		}

		CameraTarget = ActivePlayer?.ActiveGrub;

		if ( Debug )
		{
			var lineOffset = 17;
			DebugOverlay.ScreenText( $"ActivePlayer {ActivePlayer}", lineOffset++ );
			DebugOverlay.ScreenText( $"ActiveGrub {ActivePlayer?.ActiveGrub}", lineOffset++ );
			DebugOverlay.ScreenText( $"Turn Timer {TimeUntilTurnOver}", lineOffset++ );
			DebugOverlay.ScreenText( $"", lineOffset++ );
		}
	}

	[ConVar.Replicated( "gr_debug_ffa" )]
	public static bool Debug { get; set; } = false;
}
