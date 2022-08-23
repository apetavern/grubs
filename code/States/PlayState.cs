using Grubs.Player;
using Grubs.Utils;
using Grubs.Weapons.Projectiles;

namespace Grubs.States;

public partial class PlayState : BaseState
{
	[Net] public IList<Client> Participants { get; private set; } = new List<Client>();
	[Net] public int TeamsTurn { get; private set; } = 1;
	[Net] public bool UsedTurn { get; private set; }
	[Net] public TimeUntil TimeUntilTurnEnd { get; private set; }

	protected override void Enter( bool forced, params object[] parameters )
	{
		if ( !IsServer )
		{
			base.Enter( forced, parameters );
			return;
		}

		GameConfig.TeamIndex = 1;

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

		if ( !UsedTurn )
		{
			if ( TimeUntilTurnEnd <= 0 )
				NextTurn();

			return;
		}

		// No worms should be moving around to end the turn
		if ( All.OfType<Worm>().Any( worm => !worm.Velocity.IsNearlyZero( 0.1f ) ) )
			return;

		// All projectiles should've exploded before ending the turn
		if ( All.OfType<Projectile>().Any() )
			return;

		NextTurn();
	}

	public void UseTurn()
	{
		UsedTurn = true;
	}

	private void NextTurn()
	{
		if ( CheckState() )
			return;

		var participant = Participants[TeamsTurn++ - 1];
		if ( TeamsTurn > Participants.Count )
			TeamsTurn = 1;

		(participant.Pawn as GrubsPlayer)!.PickNextWorm();
		UsedTurn = false;
		TimeUntilTurnEnd = GameConfig.TurnDuration;
	}

	private bool CheckState()
	{
		var teamsDead = 0;
		GrubsPlayer lastTeamAlive = null;

		foreach ( var participant in Participants )
		{
			var pawn = participant.Pawn as GrubsPlayer;
			if ( pawn.Worms.Any( worm => worm.LifeState != LifeState.Dead ) )
			{
				lastTeamAlive = pawn;
				continue;
			}

			teamsDead++;
		}

		if ( teamsDead == Participants.Count )
		{
			SwitchStateTo<GameEndState>( GameResultType.Draw );
			return true;
		}

		if ( teamsDead == Participants.Count - 1 )
		{
			SwitchStateTo<GameEndState>( GameResultType.TeamWon, new GrubsPlayer[] {lastTeamAlive} );
			return true;
		}

		return false;
	}

	[ConCmd.Admin]
	public static void SkipTurn()
	{
		if ( GrubsGame.Current.CurrentState is not PlayState state )
			return;

		state.NextTurn();
	}
}
