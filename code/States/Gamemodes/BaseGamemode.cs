using System.Threading.Tasks;
using Grubs.Crates;
using Grubs.Player;
using Grubs.Terrain;
using Grubs.Terrain.Shapes;
using Grubs.Utils;
using Grubs.Utils.Event;

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
	public TeamManager TeamManager { get; private set; } = null!;

	/// <summary>
	/// Whether or not the current team has used their turn.
	/// </summary>
	[Net, Predicted]
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

		GrubsGame.Current.TerrainMap = new TerrainMap();
		GrubsGame.Current.TerrainModel = new TerrainModel();
		GrubsGame.InitializeTerrainClient( To.Everyone );

		TeamManager = new TeamManager();

		var killBoundary = new MultiShape()
			// Bottom bar
			.AddShape( BoxShape.WithSize( new Vector3( 7400, 32, 10000 ) ).WithOffset( new Vector3( -2500, 0, -10200 ) ) )
			// Left bar
			.AddShape( BoxShape.WithSize( new Vector3( 2300, 32, 10000 ) ).WithOffset( new Vector3( -2500, 0, -200 ) ) )
			// Right bar
			.AddShape( BoxShape.WithSize( new Vector3( 2300, 32, 10000 ) ).WithOffset( new Vector3( 2600, 0, -200 ) ) );
		new DamageZone()
			.WithDamageFlags( DamageFlags.Generic )
			.WithInstantKill( true )
			.WithDamage( 9999 )
			.WithPosition( Vector3.Zero )
			.WithShape( killBoundary )
			.Finish();

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
	/// Sets up all spectators to watch the game.
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
		TerrainZone.All.Clear();
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

		await GameTask.DelaySeconds( 1 );
		TeamManager.CurrentTeam.ActiveGrub.EquipWeapon( null );

		bool rerun;
		do
		{
			rerun = false;

			foreach ( var team in TeamManager.Teams )
			{
				foreach ( var grub in team.Grubs )
				{
					if ( grub.LifeState == LifeState.Dead )
						continue;

					foreach ( var zone in TerrainZone.All )
					{
						if ( !zone.InZone( grub ) )
							continue;

						zone.Trigger( grub );
					}

					if ( !grub.HasBeenDamaged )
						continue;

					rerun = true;
					if ( grub.ApplyDamage() && grub.DeathTask is not null && !grub.DeathTask.IsCompleted )
						await grub.DeathTask;

					await GameTask.Delay( 300 );
				}
			}

			while ( !IsWorldResolved() )
				await GameTask.Delay( 100 );
		} while ( rerun );

		foreach ( var zone in TerrainZone.All )
			zone.ExpireAfterTurns--;

		await GameTask.DelaySeconds( 3 );

		// TODO: Better way of deciding where the crate should appear.
		var rand = new Random( Time.Now.CeilToInt() );
		var num = rand.Next( 0, 100 );
		if ( num <= GameConfig.WeaponCrateChancePerTurn )
		{
			var x = Rand.Float( GameConfig.TerrainWidth * GameConfig.TerrainScale );
			var startZ = GameConfig.TerrainHeight * GameConfig.TerrainScale;
			var z = Rand.Float( startZ, startZ + 100 );
			var weaponCrate = new WeaponCrate { Position = new Vector3( x, 0, z ) };
			GrubsCamera.SetTarget( weaponCrate );
			await GameTask.DelaySeconds( 1 );
		}

		num = rand.Next( 0, 100 );
		if ( num <= GameConfig.HealthCrateChancePerTurn )
		{
			var x = Rand.Float( GameConfig.TerrainWidth * GameConfig.TerrainScale );
			var startZ = GameConfig.TerrainHeight * GameConfig.TerrainScale;
			var z = Rand.Float( startZ, startZ + 100 );
			var healthCrate = new HealthCrate { Position = new Vector3( x, 0, z ) };
			GrubsCamera.SetTarget( healthCrate );
			await GameTask.DelaySeconds( 1 );
		}

		while ( !IsWorldResolved() )
			await GameTask.Delay( 100 );

		return CheckState();
	}

	/// <summary>
	/// Called after the turn change occurs.
	/// </summary>
	protected virtual async Task PostTurnChange()
	{
		Host.AssertServer();

		GrubsCamera.SetTarget( TeamManager.CurrentTeam.ActiveGrub );
		EventRunner.RunLocal( GrubsEvent.TurnChangedEvent, TeamManager.CurrentTeam );
		TurnChangedRpc( To.Everyone, TeamManager.CurrentTeam );
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
			SwitchStateTo<GameEndState>( GameResultType.TeamWon, lastTeamAlive!.TeamName, lastTeamAlive.Clients );
			return true;
		}

		return false;
	}

	[ClientRpc]
	private void TurnChangedRpc( Team newTeam )
	{
		EventRunner.RunLocal( GrubsEvent.TurnChangedEvent, newTeam );
	}
}
