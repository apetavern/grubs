using Grubs.Player;
using Grubs.Utils;
using Grubs.Weapons.Projectiles;

namespace Grubs.States;

public abstract partial class BaseGamemode : BaseState
{
	[Net]
	public TeamManager TeamManager { get; private set; }
	[Net]
	public bool UsedTurn { get; private set; }
	[Net]
	public TimeUntil TimeUntilTurnEnd { get; private set; }

	protected override void Enter( bool forced, params object[] parameters )
	{
		if ( !IsServer )
		{
			base.Enter( forced, parameters );
			return;
		}

		TeamManager = new TeamManager();

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
		SetupParticipants( participants );

		// Setup spectators.
		var spectators = new List<Client>();
		foreach ( var client in Client.All )
		{
			if ( participants.Contains( client ) )
				continue;

			spectators.Add( client );
		}
		SetupSpectators( spectators );

		// Start the game.
		NextTurn();

		base.Enter( forced, parameters );
	}

	protected abstract void SetupParticipants( List<Client> participants );

	protected virtual void SetupSpectators( List<Client> spectators )
	{
		foreach ( var spectator in spectators )
			spectator.Pawn = new Spectator();
	}

	protected override void Leave()
	{
		if ( !IsServer )
		{
			base.Leave();
			return;
		}

		TeamManager.Delete();
		foreach ( var client in Client.All )
			client.Pawn?.Delete();

		base.Leave();
	}

	public override void ClientDisconnected( Client cl, NetworkDisconnectionReason reason )
	{
		base.ClientDisconnected( cl, reason );

		// Bail from the game if one of the participants leaves.
		if ( TeamManager.Teams.SelectMany( team => team.Clients ).Any( player => player.Client == cl ) )
			SwitchStateTo<GameEndState>( GameResultType.Abandoned, reason.ToString() );
	}

	public override void Tick()
	{
		base.Tick();

		if ( !IsServer )
			return;

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

	public virtual void UseTurn()
	{
		Host.AssertServer();

		UsedTurn = true;
	}

	public virtual void NextTurn()
	{
		Host.AssertServer();

		TeamManager.CurrentTeam.ActiveWorm.EquipWeapon( null );

		if ( CheckState() )
			return;

		TeamManager.Cycle();
		UsedTurn = false;
		TimeUntilTurnEnd = GameConfig.TurnDuration;
	}

	protected virtual bool CheckState()
	{
		Host.AssertServer();

		var teamsDead = 0;
		Team lastTeamAlive = null;

		foreach ( var team in TeamManager.Teams )
		{
			if ( team.TeamDead )
			{
				teamsDead++;
				continue;
			}

			lastTeamAlive = team;
		}

		if ( teamsDead == TeamManager.Teams.Count )
		{
			SwitchStateTo<GameEndState>( GameResultType.Draw );
			return true;
		}

		if ( teamsDead == TeamManager.Teams.Count - 1 )
		{
			SwitchStateTo<GameEndState>( GameResultType.TeamWon, lastTeamAlive.Clients );
			return true;
		}

		return false;
	}
}
