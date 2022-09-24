using System.IO;
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
	/// The current gamemode the game is in.
	/// </summary>
	public new static BaseGamemode? Instance => BaseState.Instance as BaseGamemode;

	/// <summary>
	/// The manager entity for teams of grubs.
	/// </summary>
	[Net]
	public TeamManager TeamManager { get; private set; } = null!;
	/// <summary>
	/// The terrain map for the game.
	/// </summary>
	[Net]
	public TerrainMap TerrainMap { get; private set; } = null!;

	/// <summary>
	/// Whether or not the active Grub can only use movement.
	/// </summary>
	[Net, Predicted]
	public bool MovementOnly { get; private set; }
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
	/// Whether or not a turn change is currently happening.
	/// </summary>
	[Net]
	public bool IsTurnChanging { get; private set; }
	/// <summary>
	/// The current amount of wind steps.
	/// </summary>
	[Net]
	public int WindSteps { get; set; }
	/// <summary>
	/// The current wind force.
	/// </summary>
	public float Wind => WindSteps * GameConfig.WindForce;

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

		SetupTerrain();
		TeamManager = new TeamManager();

		var killBoundary = new MultiShape()
			// Bottom bar
			.AddShape( BoxShape.WithSize( new Vector3( int.MaxValue, 100, 100 ) ).WithOffset( new Vector3( (-int.MaxValue / 2), -50, -100 ) ) );

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
	/// Sets up the terrain the teams will play in.
	/// </summary>
	protected virtual void SetupTerrain()
	{
		if ( GameConfig.TerrainFile != string.Empty )
		{
			var terrainFile = GameConfig.TerrainFile;
			BinaryReader? reader = null;
			try
			{
				if ( FileSystem.Mounted.FileExists( terrainFile ) )
				{
					reader = new BinaryReader( FileSystem.Mounted.OpenRead( terrainFile ) );
					TerrainMap = new TerrainMap( PremadeTerrain.Deserialize( reader ) );
				}
				else if ( FileSystem.Data.FileExists( terrainFile ) )
				{
					reader = new BinaryReader( FileSystem.Data.OpenRead( terrainFile ) );
					TerrainMap = new TerrainMap( PremadeTerrain.Deserialize( reader ) );
				}
				else
				{
					Log.Error( $"Map \"{terrainFile}\" does not exist. Reverting to random gen" );
					TerrainMap = new TerrainMap( Rand.Int( 99999 ) );
				}
			}
			catch ( Exception e )
			{
				Log.Error( e );
			}
			finally
			{
				reader?.Close();
			}
		}
		else
			TerrainMap = new TerrainMap( Rand.Int( 99999 ) );
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

	public override void ClientJoined( Client cl )
	{
		base.ClientJoined( cl );

		TerrainMap.UpdateGridRpc( TerrainMap.TerrainGrid );
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

		var damageables = All.OfType<IDamageable>().ToArray();
		foreach ( var damageable in damageables )
		{
			if ( damageable is not Entity { IsValid: true } entity )
				continue;

			foreach ( var zone in TerrainZone.All.OfType<DamageZone>() )
			{
				if ( !zone.IsValid || !zone.InstantKill || !zone.InZone( entity ) )
					continue;

				if ( entity is Grub { IsTurn: true } )
					UseTurn( false );

				zone.Trigger( entity );
				damageable.ApplyDamage();
			}
		}

		if ( !UsedTurn && TimeUntilTurnEnd <= 0 )
			UseTurn( false );

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
	/// <param name="giveMovementGrace">Whether or not to give the Grub a movement grace period.</param>
	public virtual void UseTurn( bool giveMovementGrace )
	{
		if ( giveMovementGrace )
		{
			TimeUntilTurnEnd = GameConfig.MovementGracePeriod;
			MovementOnly = true;
		}
		else
			UsedTurn = true;
	}

	/// <summary>
	/// Moves the game to the next turn.
	/// </summary>
	public virtual async Task NextTurn()
	{
		Host.AssertServer();

		IsTurnChanging = true;
		if ( await PreTurnChange() )
			return;

		TeamManager.Cycle();
		UsedTurn = false;
		TimeUntilTurnEnd = GameConfig.TurnDuration;
		IsTurnChanging = false;

		if ( GameConfig.WindEnabled )
			WindSteps = Rand.Int( -GameConfig.WindSteps, GameConfig.WindSteps );

		EventRunner.RunLocal( GrubsEvent.TurnChangedEvent, TeamManager.CurrentTeam );
		TurnChangedRpc( To.Everyone, TeamManager.CurrentTeam );

		await PostTurnChange();
	}

	/// <summary>
	/// Called before the turn change occurs. Handle cleanup and check game state here.
	/// </summary>
	/// <returns>Whether or not to bail from changing to the next turn.</returns>
	protected virtual async ValueTask<bool> PreTurnChange()
	{
		Host.AssertServer();

		MovementOnly = false;
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
			var x = Rand.Float( TerrainMap.Width * TerrainMap.Scale );
			var startZ = TerrainMap.Height * TerrainMap.Scale;
			var z = Rand.Float( startZ, startZ + 100 );
			var weaponCrate = new WeaponCrate { Position = new Vector3( x, 0, z ) };
			await GameTask.DelaySeconds( 1 );
		}

		num = rand.Next( 0, 100 );
		if ( num <= GameConfig.HealthCrateChancePerTurn )
		{
			var x = Rand.Float( TerrainMap.Width * TerrainMap.Scale );
			var startZ = TerrainMap.Height * TerrainMap.Scale;
			var z = Rand.Float( startZ, startZ + 100 );
			var healthCrate = new HealthCrate { Position = new Vector3( x, 0, z ) };
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

		var lastUsedWeapon = TeamManager.CurrentTeam.Inventory.LastUsedWeapon;
		if ( lastUsedWeapon is not null )
		{
			if ( lastUsedWeapon.Ammo != 0 )
				TeamManager.CurrentTeam.ActiveGrub.EquipWeapon( lastUsedWeapon );
			else
				TeamManager.CurrentTeam.Inventory.LastUsedWeapon = null;
		}
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
