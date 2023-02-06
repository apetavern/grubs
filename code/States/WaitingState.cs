using Grubs.Utils;
using Grubs.Utils.Extensions;

namespace Grubs.States;

/// <summary>
/// A simple state to sit in while preparing for the next gamemode.
/// </summary>
public sealed partial class WaitingState : BaseState
{
	/// <summary>
	/// The time until the state will be switched to the gamemode.
	/// </summary>
	[Net]
	public TimeUntil TimeUntilStart { get; private set; }

	/// <summary>
	/// Whether or not the countdown in <see cref="TimeUntilStart"/> has been started.
	/// </summary>
	public bool IsStarting => Game.Clients.Count >= GameConfig.MinimumPlayers && TimeUntilStart > 0;

	protected override void Enter( bool forced, params object[] parameters )
	{
		base.Enter( forced, parameters );

		Log.Info( "Enter WaitingState" );
		if ( Game.Clients.Count >= GameConfig.MinimumPlayers )
			TimeUntilStart = 10;
	}

	public override void ClientJoined( IClient cl )
	{
		base.ClientJoined( cl );

		if ( Game.Clients.Count >= GameConfig.MinimumPlayers && Game.Clients.Count <= GameConfig.MaximumPlayers )
			TimeUntilStart = 10;

		if ( cl.IsBot )
			TimeUntilStart = 0f;
	}

	public override void ClientDisconnected( IClient cl, NetworkDisconnectionReason reason )
	{
		base.ClientDisconnected( cl, reason );

		if ( Game.Clients.Count < GameConfig.MinimumPlayers )
			TimeUntilStart = -1;
	}

	public override void Tick()
	{
		base.Tick();

		if ( !Game.IsServer || Game.Clients.Count < GameConfig.MinimumPlayers || TimeUntilStart > 0 )
			return;

		// TODO: UI for getting participants?
		var participants = new List<IClient>();
		for ( var i = 0; i < Math.Min( Game.Clients.Count, GameConfig.MaximumPlayers ); i++ )
			participants.Add( Game.Clients.GetByIndex( i ) );

		switch ( GameConfig.Gamemode )
		{
			case "ffa":
				SwitchStateTo<FreeForAll>( participants );
				break;
			case "tdm":
				SwitchStateTo<TeamDeathmatch>( participants );
				break;
			default:
				Log.Error( $"Got unknown gamemode \"{GameConfig.Gamemode}\"" );
				break;
		}
	}
}
