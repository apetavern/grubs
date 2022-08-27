using Grubs.Utils;

namespace Grubs.States;

public partial class WaitingState : BaseState
{
	[Net] public TimeUntil TimeUntilStart { get; private set; }
	public bool IsStarting => Client.All.Count >= GameConfig.MinimumPlayers && TimeUntilStart > 0;

	public override void ClientJoined( Client cl )
	{
		base.ClientJoined( cl );

		if ( Client.All.Count >= GameConfig.MinimumPlayers && Client.All.Count <= GameConfig.MaximumPlayers )
			TimeUntilStart = 10;
	}

	public override void ClientDisconnected( Client cl, NetworkDisconnectionReason reason )
	{
		base.ClientDisconnected( cl, reason );

		if ( Client.All.Count < GameConfig.MinimumPlayers )
			TimeUntilStart = -1;
	}

	public override void Tick()
	{
		base.Tick();

		if ( Client.All.Count < GameConfig.MinimumPlayers || TimeUntilStart > 0 )
			return;

		// TODO: UI for getting participants?
		var participants = new List<Client>();
		for ( var i = 0; i < Math.Min( Client.All.Count, GameConfig.MaximumPlayers ); i++ )
			participants.Add( Client.All[i] );

		switch ( GameConfig.Gamemode )
		{
			case "ffa":
				SwitchStateTo<FreeForAll>( participants );
				break;
			default:
				Log.Error( $"Got unknown gamemode \"{GameConfig.Gamemode}\"" );
				break;
		}
	}
}
