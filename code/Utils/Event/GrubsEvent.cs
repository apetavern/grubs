using Grubs.States;

namespace Grubs.Utils.Event;

/// <summary>
/// A collection of events that are specific to Grubs.
/// </summary>
public static class GrubsEvent
{
	/// <summary>
	/// The prefix for any event that is only called on the client realm.
	/// </summary>
	public const string ClientPrefix = "Client.";
	/// <summary>
	/// The prefix for any event that is only called on the server realm.
	/// </summary>
	public const string ServerPrefix = "Server.";

	#region EnterState
	/// <summary>
	/// Called when the game enters a new state.
	/// </summary>
	public const string EnterStateEvent = "grubs_enter-state";

	/// <summary>
	/// Called when the game enters a new state.
	/// </summary>
	[AttributeUsage( AttributeTargets.Method )]
	public class EnterStateAttribute : EventAttribute
	{
		public EnterStateAttribute( string state = null ) : base( EnterStateEvent + state ?? string.Empty )
		{
			if ( state is null )
				return;

			AssertValidState( state );
		}
	}

	/// <summary>
	/// Called when the game enters a new state.
	/// </summary>
	public static class EnterState
	{
		/// <summary>
		/// <see cref="EnterState"/> event but only called on the client realm.
		/// </summary>
		public const string ClientEvent = ClientPrefix + EnterStateEvent;
		/// <summary>
		/// <see cref="EnterState"/> event but only called on the server realm.
		/// </summary>
		public const string ServerEvent = ServerPrefix + EnterStateEvent;

		/// <summary>
		/// <see cref="EnterState"/> event but only called on the client realm.
		/// </summary>
		[AttributeUsage( AttributeTargets.Method )]
		public class ClientAttribute : EventAttribute
		{
			public ClientAttribute( string state = null ) : base( ClientEvent + state ?? string.Empty )
			{
				if ( state is null )
					return;

				AssertValidState( state );
			}
		}

		/// <summary>
		/// <see cref="EnterState"/> event but only called on the server realm.
		/// </summary>
		[AttributeUsage( AttributeTargets.Method )]
		public class ServerAttribute : EventAttribute
		{
			public ServerAttribute( string state = null ) : base( ServerEvent + state ?? string.Empty )
			{
				if ( state is null )
					return;

				AssertValidState( state );
			}
		}
	}
	#endregion

	#region LeaveState
	/// <summary>
	/// Called when the game leaves its current state.
	/// </summary>
	public const string LeaveStateEvent = "grubs_leave-state";

	/// <summary>
	/// Called when the game leaves its current state.
	/// </summary>
	[AttributeUsage( AttributeTargets.Method )]
	public class LeaveStateAttribute : EventAttribute
	{
		public LeaveStateAttribute( string state = null ) : base( LeaveStateEvent + state ?? string.Empty )
		{
			if ( state is null )
				return;

			AssertValidState( state );
		}
	}

	/// <summary>
	/// Called when the game leaves its current state.
	/// </summary>
	public static class LeaveState
	{
		/// <summary>
		/// <see cref="LeaveState"/> event but only called on the client realm.
		/// </summary>
		public const string ClientEvent = ClientPrefix + LeaveStateEvent;
		/// <summary>
		/// <see cref="LeaveState"/> event but only called on the server realm.
		/// </summary>
		public const string ServerEvent = ServerPrefix + LeaveStateEvent;

		/// <summary>
		/// <see cref="LeaveState"/> event but only called on the client realm.
		/// </summary>
		[AttributeUsage( AttributeTargets.Method )]
		public class ClientAttribute : EventAttribute
		{
			public ClientAttribute( string state = null ) : base( ClientEvent + state ?? string.Empty )
			{
				if ( state is null )
					return;

				AssertValidState( state );
			}
		}

		/// <summary>
		/// <see cref="LeaveState"/> event but only called on the server realm.
		/// </summary>
		[AttributeUsage( AttributeTargets.Method )]
		public class ServerAttribute : EventAttribute
		{
			public ServerAttribute( string state = null ) : base( ServerEvent + state ?? string.Empty )
			{
				if ( state is null )
					return;

				AssertValidState( state );
			}
		}
	}
	#endregion

	#region WormHurt
	/// <summary>
	/// Called when a worm gets hurt.
	/// </summary>
	public const string WormHurtEvent = "grubs_worm-hurt";

	/// <summary>
	/// Called when a worm gets hurt.
	/// </summary>
	[AttributeUsage( AttributeTargets.Method )]
	public class WormHurtAttribute : EventAttribute
	{
		public WormHurtAttribute( string state = null ) : base( WormHurtEvent + state ?? string.Empty )
		{
			if ( state is null )
				return;

			AssertValidState( state );
		}
	}

	/// <summary>
	/// Called when a worm gets hurt.
	/// </summary>
	public static class WormHurt
	{
		/// <summary>
		/// <see cref="WormHurt"/> event but only called on the client realm.
		/// </summary>
		public const string ClientEvent = ClientPrefix + WormHurtEvent;
		/// <summary>
		/// <see cref="WormHurt"/> event but only called on the server realm.
		/// </summary>
		public const string ServerEvent = ServerPrefix + WormHurtEvent;

		/// <summary>
		/// <see cref="WormHurt"/> event but only called on the client realm.
		/// </summary>
		[AttributeUsage( AttributeTargets.Method )]
		public class ClientAttribute : EventAttribute
		{
			public ClientAttribute( string state = null ) : base( ClientEvent + state ?? string.Empty )
			{
				if ( state is null )
					return;

				AssertValidState( state );
			}
		}

		/// <summary>
		/// <see cref="WormHurt"/> event but only called on the server realm.
		/// </summary>
		[AttributeUsage( AttributeTargets.Method )]
		public class ServerAttribute : EventAttribute
		{
			public ServerAttribute( string state = null ) : base( ServerEvent + state ?? string.Empty )
			{
				if ( state is null )
					return;

				AssertValidState( state );
			}
		}
	}
	#endregion

	/// <summary>
	/// Verifies that if a state was passed to the <see cref="EnterState"/> or <see cref="LeaveState"/> event that it is an existing <see cref="BaseState"/>.
	/// </summary>
	/// <param name="state">The state that is being verified.</param>
	private static void AssertValidState( string state )
	{
		// TODO: Events get created outside of the game so the TypeLibrary will be invalid. Fix this if changed.
		//var type = TypeLibrary.GetTypeByName( state );
		//Assert.True( type is not null && type.IsAssignableTo( typeof(BaseState) ) );
	}
}
