using System.Threading.Tasks;
using Grubs.Player;
using Grubs.Terrain;
using Grubs.Utils;
using Grubs.Utils.Event;
using Grubs.Utils.Extensions;

namespace Grubs.States;

/// <summary>
/// Base state for any gameplay loop.
/// </summary>
public abstract partial class BaseGamemode : BaseState
{
	/// <summary>
	/// The manager entity for teams of grubs.
	/// </summary>
	[Net]
	protected TeamManager TeamManager { get; private set; }
	/// <summary>
	/// Whether or not the current team has used their turn.
	/// </summary>
	[Net]
	public bool UsedTurn { get; private set; }
	/// <summary>
	/// The time until the current teams turn is automatically ended.
	/// </summary>
	[Net]
	public TimeUntil TimeUntilTurnEnd { get; private set; }

	/// <summary>
	/// The async task for switching to the next turn.
	/// </summary>
	public Task? NextTurnTask;

	protected override void Enter( bool forced, params object[] parameters )
	{
		if ( !IsServer )
		{
			base.Enter( forced, parameters );
			return;
		}

		TeamManager = new TeamManager();
		DamageZone.All.Add( new DamageZone
		{
			Position = new Vector3( -750, 0, -200 ),
			Size = new Vector3( 5000, 32, 32 )
		} );

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
		NextTurnTask = NextTurn();

		base.Enter( forced, parameters );
	}

	/// <summary>
	/// Sets up all participants to play the game.
	/// </summary>
	/// <param name="participants"></param>
	protected abstract void SetupParticipants( List<Client> participants );

	/// <summary>
	/// Sets upp all spectators to watch the game.
	/// </summary>
	/// <param name="spectators"></param>
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
		DamageZone.All.Clear();
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

		if ( !UsedTurn && TimeUntilTurnEnd <= 0 )
			UseTurn();

		if ( UsedTurn && IsWorldResolved() && (NextTurnTask is null || NextTurnTask.IsCompleted) )
			NextTurnTask = NextTurn();
	}

	protected override void RunEnterEvents()
	{
		EventRunner.RunLocal( GrubsEvent.EnterGamemodeEvent + GetType().Name );
		EventRunner.RunLocal( GrubsEvent.EnterGamemodeEvent );

		base.RunEnterEvents();
	}

	protected override void RunLeaveEvents()
	{
		EventRunner.RunLocal( GrubsEvent.LeaveGamemodeEvent + GetType().Name );
		EventRunner.RunLocal( GrubsEvent.LeaveGamemodeEvent );

		base.RunLeaveEvents();
	}

	/// <summary>
	/// Uses the current teams turn.
	/// </summary>
	public virtual void UseTurn()
	{
		Host.AssertServer();

		UsedTurn = true;
	}

	/// <summary>
	/// Moves the game to the next turn.
	/// </summary>
	public virtual async Task NextTurn()
	{
		Host.AssertServer();

		if ( await PreTurnChange() )
			return;

		TeamManager.Cycle();
		UsedTurn = false;
		TimeUntilTurnEnd = GameConfig.TurnDuration;

		await PostTurnChange();
	}

	/// <summary>
	/// Called before the turn change occurs. Handle cleanup and check game state here.
	/// </summary>
	/// <returns>Whether or not to bail from changing to the next turn.</returns>
	protected virtual async ValueTask<bool> PreTurnChange()
	{
		Host.AssertServer();

		UsedTurn = true;

		TeamManager.CurrentTeam.ActiveGrub.EquipWeapon( null );
		foreach ( var team in TeamManager.Teams )
		{
			foreach ( var grub in team.Grubs )
			{
				foreach ( var zone in DamageZone.All )
				{
					if ( !zone.InZone( grub ) )
						continue;

					var damageInfo = DamageInfoExtension.FromZone( zone );
					damageInfo.Position = grub.Position;
					grub.TakeDamage( damageInfo );
				}

				if ( !grub.HasBeenDamaged )
					continue;

				grub.ApplyDamage();
				await GameTask.Delay( 300 );
			}
		}

		while ( !IsWorldResolved() )
			await GameTask.Delay( 100 );

		await GameTask.DelaySeconds( 3 );
		return CheckState();
	}

	/// <summary>
	/// Called after the turn change occurs.
	/// </summary>
	protected virtual async Task PostTurnChange()
	{
		Host.AssertServer();

		GrubsCamera.SetTarget( TeamManager.CurrentTeam.ActiveGrub );
	}

	/// <summary>
	/// Checks whether or not the world is ready to move to the next turn.
	/// </summary>
	/// <returns>Whether or not the world is ready to move to the next turn.</returns>
	protected virtual bool IsWorldResolved()
	{
		return All.OfType<IResolvable>().All( entity => entity.Resolved );
	}

	/// <summary>
	/// Checks the state of the game. Check your win conditions here.
	/// </summary>
	/// <returns>Whether or not the game has concluded.</returns>
	protected virtual bool CheckState()
	{
		Host.AssertServer();

		var teamsDead = 0;
		Team? lastTeamAlive = null;

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
			SwitchStateTo<GameEndState>( GameResultType.TeamWon, lastTeamAlive!.Clients );
			return true;
		}

		return false;
	}
}
