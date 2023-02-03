namespace Grubs;

public partial class FreeForAll : Gamemode
{
	public override string GamemodeName => "Free For All";
	public override bool AllowFriendlyFire => true;
	public override int MinimumPlayers => GrubsConfig.MinimumPlayers;

	public IList<Player> PlayerRotation { get; set; } = new List<Player>();

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

	}
}
