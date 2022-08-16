using Grubs.Player;
using Grubs.Utils;

namespace Grubs.States;

public partial class PlayState : BaseState
{
	[Net] public IList<Client> Participants { get; private set; } = new List<Client>();
	[Net] public int TeamsTurn { get; set; } = 1;
	[Net] public TimeUntil TimeUntilTurnEnd { get; private set; }

	protected override void Enter( bool forced, params object[] parameters )
	{
		if ( !IsServer )
		{
			base.Enter( forced, parameters );
			return;
		}

		List<Client> participants;
		if ( forced )
		{
			// Just grab some participants.
			participants = new List<Client>();
			for ( var i = 0; i < Math.Min( Client.All.Count, GameConfig.MaximumPlayers ); i++ )
				participants.Add( Client.All[i] );
		}
		else
			participants = (List<Client>)parameters[0];

		// Setup participants.
		foreach ( var participant in participants )
		{
			var player = new GrubsPlayer( participant );
			participant.Pawn = player;

			player.Spawn();
		}

		// Setup spectators.
		foreach ( var client in Client.All )
		{
			if ( participants.Contains( client ) )
				continue;

			// TODO: Spectator pawn?
		}

		// Start the game.
		Participants = participants;
		NextTurn();
		
		base.Enter( forced, parameters );
	}

	protected override void Leave()
	{
		if ( !IsServer )
		{
			base.Leave();
			return;
		}
		
		foreach ( var client in Client.All )
			client.Pawn?.Delete();
		
		base.Leave();
	}

	public override void ClientDisconnected( Client cl, NetworkDisconnectionReason reason )
	{
		base.ClientDisconnected( cl, reason );

		// Bail from the game if one of the participants leaves.
		foreach ( var participant in Participants )
		{
			if ( participant == cl )
				SwitchStateTo<GameEndState>( GameResultType.Abandoned );
		}
	}

	public override void Tick()
	{
		base.Tick();
		
		if ( TimeUntilTurnEnd <= 0 )
			NextTurn();
	}

	private void NextTurn()
	{
		var participant = Participants[TeamsTurn++ - 1];
		if ( TeamsTurn > Participants.Count )
			TeamsTurn = 1;
		
		(participant.Pawn as GrubsPlayer)!.PickNextWorm();
		TimeUntilTurnEnd = GameConfig.TurnDuration;
	}
}
