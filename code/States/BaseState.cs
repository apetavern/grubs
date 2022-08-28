using Grubs.Utils.Event;

namespace Grubs.States;

/// <summary>
/// Base class for any state the game is in.
/// </summary>
[Category( "Setup" )]
public abstract partial class BaseState : Entity
{
	private bool _entered;
	private bool _forced;

	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Always;
	}

	/// <summary>
	/// Called when we've just switched to this state.
	/// <remarks>If overriding, you should call the base at the end of your method.</remarks>
	/// </summary>
	/// <param name="forced">Whether or not the change in game state was forced. Like from a console command.</param>
	/// <param name="parameters">An array of parameters passed from the previous game state.</param>
	protected virtual void Enter( bool forced, params object[] parameters )
	{
		if ( _entered )
			return;

		_entered = true;
		_forced = forced;
		RunEnterEvents();

		if ( IsServer )
			EnterGameRpc( To.Everyone, forced );
	}

	/// <summary>
	/// Called when we're leaving this state.
	/// <remarks>If overriding, you should call the base at the end of your method.</remarks>
	/// </summary>
	protected virtual void Leave()
	{
		RunLeaveEvents();

		if ( IsServer )
			LeaveGameRpc( To.Everyone );
	}

	/// <summary>
	/// Called when a new client has joined the game.
	/// </summary>
	/// <param name="cl">The client that has joined.</param>
	public virtual void ClientJoined( Client cl )
	{
		Host.AssertServer();

		EnterGameRpc( To.Single( cl ), _forced );
	}

	/// <summary>
	/// Called when a client has disconnected from the game.
	/// </summary>
	/// <param name="cl">The client that has disconnected.</param>
	/// <param name="reason">The reason for the client disconnecting.</param>
	public virtual void ClientDisconnected( Client cl, NetworkDisconnectionReason reason )
	{
		Host.AssertServer();
	}

	/// <summary>
	/// Called each server tick and client frame.
	/// </summary>
	public virtual void Tick()
	{
	}

	// TODO: Somehow receivers of the events get called twice?
	private void RunEnterEvents()
	{
		EventRunner.RunLocal( GrubsEvent.EnterStateEvent + GetType().Name );
		EventRunner.RunLocal( GrubsEvent.EnterStateEvent );
	}

	private void RunLeaveEvents()
	{
		EventRunner.RunLocal( GrubsEvent.LeaveStateEvent + GetType().Name );
		EventRunner.RunLocal( GrubsEvent.LeaveStateEvent );
	}

	[ClientRpc]
	private void EnterGameRpc( bool force )
	{
		GrubsGame.Current.CurrentState = this;
		Enter( force );
	}

	[ClientRpc]
	private void LeaveGameRpc()
	{
		Leave();
	}

	/// <summary>
	/// Changes the game state.
	/// </summary>
	/// <typeparam name="T">The new state to switch to.</typeparam>
	protected static void SwitchStateTo<T>( params object[] parameters ) where T : BaseState
	{
		Host.AssertServer();

		SwitchStateTo( TypeLibrary.Create<T>( typeof( T ) ), false, parameters );
	}

	/// <summary>
	/// Changes the game state.
	/// </summary>
	/// <param name="state">The new state to switch to.</param>
	/// <param name="force">Whether or not this change is being forced. Like from a console command.</param>
	/// <param name="parameters">An array of parameters to send to the next game state being entered.</param>
	private static void SwitchStateTo( BaseState state, bool force, params object[] parameters )
	{
		Host.AssertServer();

		var currentState = GrubsGame.Current.CurrentState;
		currentState?.Leave();
		GrubsGame.Current.CurrentState = state;
		state?.Enter( force, parameters );

		currentState?.Delete();
	}

	[ConCmd.Admin( "state" )]
	public static void ForceStateCmd( string stateName )
	{
		var stateType = TypeLibrary.GetTypeByName( stateName );
		if ( stateType is null )
		{
			Log.Error( $"No state exists with the type name \"{stateName}\"" );
			return;
		}

		SwitchStateTo( TypeLibrary.Create<BaseState>( stateType ), true );
	}
}
