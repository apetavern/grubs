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
		public EnterStateAttribute( string? state = null ) : base( EnterStateEvent + state ?? string.Empty )
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
			public ClientAttribute( string? state = null ) : base( ClientEvent + state ?? string.Empty )
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
			public ServerAttribute( string? state = null ) : base( ServerEvent + state ?? string.Empty )
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
		public LeaveStateAttribute( string? state = null ) : base( LeaveStateEvent + state ?? string.Empty )
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
			public ClientAttribute( string? state = null ) : base( ClientEvent + state ?? string.Empty )
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
			public ServerAttribute( string? state = null ) : base( ServerEvent + state ?? string.Empty )
			{
				if ( state is null )
					return;

				AssertValidState( state );
			}
		}
	}
	#endregion

	#region EnterGamemode
	/// <summary>
	/// Called when the game enters a gamemode.
	/// </summary>
	public const string EnterGamemodeEvent = "grubs_enter-gamemode";

	/// <summary>
	/// Called when the game enters a gamemode.
	/// </summary>
	[AttributeUsage( AttributeTargets.Method )]
	public class EnterGamemodeAttribute : EventAttribute
	{
		public EnterGamemodeAttribute( string? state = null ) : base( EnterGamemodeEvent + state ?? string.Empty )
		{
			if ( state is null )
				return;

			AssertValidState( state );
		}
	}

	/// <summary>
	/// Called when the game enters a gamemode.
	/// </summary>
	public static class EnterGamemode
	{
		/// <summary>
		/// <see cref="EnterGamemode"/> event but only called on the client realm.
		/// </summary>
		public const string ClientEvent = ClientPrefix + EnterGamemodeEvent;
		/// <summary>
		/// <see cref="EnterGamemode"/> event but only called on the server realm.
		/// </summary>
		public const string ServerEvent = ServerPrefix + EnterGamemodeEvent;

		/// <summary>
		/// <see cref="EnterGamemode"/> event but only called on the client realm.
		/// </summary>
		[AttributeUsage( AttributeTargets.Method )]
		public class ClientAttribute : EventAttribute
		{
			public ClientAttribute( string? state = null ) : base( ClientEvent + state ?? string.Empty )
			{
				if ( state is null )
					return;

				AssertValidState( state );
			}
		}

		/// <summary>
		/// <see cref="EnterGamemode"/> event but only called on the server realm.
		/// </summary>
		[AttributeUsage( AttributeTargets.Method )]
		public class ServerAttribute : EventAttribute
		{
			public ServerAttribute( string? state = null ) : base( ServerEvent + state ?? string.Empty )
			{
				if ( state is null )
					return;

				AssertValidState( state );
			}
		}
	}
	#endregion

	#region LeaveGamemode
	/// <summary>
	/// Called when the game leaves a gamemode.
	/// </summary>
	public const string LeaveGamemodeEvent = "grubs_leave-gamemode";

	/// <summary>
	/// Called when the game leaves a gamemode.
	/// </summary>
	[AttributeUsage( AttributeTargets.Method )]
	public class LeaveGamemodeAttribute : EventAttribute
	{
		public LeaveGamemodeAttribute( string? state = null ) : base( LeaveGamemodeEvent + state ?? string.Empty )
		{
			if ( state is null )
				return;

			AssertValidState( state );
		}
	}

	/// <summary>
	/// Called when the game leaves a gamemode.
	/// </summary>
	public static class LeaveGamemode
	{
		/// <summary>
		/// <see cref="LeaveGamemode"/> event but only called on the client realm.
		/// </summary>
		public const string ClientEvent = ClientPrefix + LeaveGamemodeEvent;
		/// <summary>
		/// <see cref="LeaveGamemode"/> event but only called on the server realm.
		/// </summary>
		public const string ServerEvent = ServerPrefix + LeaveGamemodeEvent;

		/// <summary>
		/// <see cref="LeaveGamemode"/> event but only called on the client realm.
		/// </summary>
		[AttributeUsage( AttributeTargets.Method )]
		public class ClientAttribute : EventAttribute
		{
			public ClientAttribute( string? state = null ) : base( ClientEvent + state ?? string.Empty )
			{
				if ( state is null )
					return;

				AssertValidState( state );
			}
		}

		/// <summary>
		/// <see cref="LeaveGamemode"/> event but only called on the server realm.
		/// </summary>
		[AttributeUsage( AttributeTargets.Method )]
		public class ServerAttribute : EventAttribute
		{
			public ServerAttribute( string? state = null ) : base( ServerEvent + state ?? string.Empty )
			{
				if ( state is null )
					return;

				AssertValidState( state );
			}
		}
	}
	#endregion

	#region GrubHurt
	/// <summary>
	/// Called when a grub gets hurt.
	/// </summary>
	public const string GrubHurtEvent = "grubs_grub-hurt";

	/// <summary>
	/// Called when a grub gets hurt.
	/// </summary>
	[AttributeUsage( AttributeTargets.Method )]
	public class GrubHurtAttribute : EventAttribute
	{
		public GrubHurtAttribute( string? state = null ) : base( GrubHurtEvent + state ?? string.Empty )
		{
			if ( state is null )
				return;

			AssertValidState( state );
		}
	}

	/// <summary>
	/// Called when a grub gets hurt.
	/// </summary>
	public static class GrubHurt
	{
		/// <summary>
		/// <see cref="GrubHurt"/> event but only called on the client realm.
		/// </summary>
		public const string ClientEvent = ClientPrefix + GrubHurtEvent;
		/// <summary>
		/// <see cref="GrubHurt"/> event but only called on the server realm.
		/// </summary>
		public const string ServerEvent = ServerPrefix + GrubHurtEvent;

		/// <summary>
		/// <see cref="GrubHurt"/> event but only called on the client realm.
		/// </summary>
		[AttributeUsage( AttributeTargets.Method )]
		public class ClientAttribute : EventAttribute
		{
			public ClientAttribute( string? state = null ) : base( ClientEvent + state ?? string.Empty )
			{
				if ( state is null )
					return;

				AssertValidState( state );
			}
		}

		/// <summary>
		/// <see cref="GrubHurt"/> event but only called on the server realm.
		/// </summary>
		[AttributeUsage( AttributeTargets.Method )]
		public class ServerAttribute : EventAttribute
		{
			public ServerAttribute( string? state = null ) : base( ServerEvent + state ?? string.Empty )
			{
				if ( state is null )
					return;

				AssertValidState( state );
			}
		}
	}
	#endregion

	#region GrubDied
	/// <summary>
	/// Called when a grub has died.
	/// </summary>
	public const string GrubDiedEvent = "grubs_grub-died";

	/// <summary>
	/// Called when a grub has died.
	/// </summary>
	[AttributeUsage( AttributeTargets.Method )]
	public class GrubDiedAttribute : EventAttribute
	{
		public GrubDiedAttribute( string? state = null ) : base( GrubDiedEvent + state ?? string.Empty )
		{
			if ( state is null )
				return;

			AssertValidState( state );
		}
	}

	/// <summary>
	/// Called when a grub has died.
	/// </summary>
	public static class GrubDied
	{
		/// <summary>
		/// <see cref="GrubDied"/> event but only called on the client realm.
		/// </summary>
		public const string ClientEvent = ClientPrefix + GrubDiedEvent;
		/// <summary>
		/// <see cref="GrubDied"/> event but only called on the server realm.
		/// </summary>
		public const string ServerEvent = ServerPrefix + GrubDiedEvent;

		/// <summary>
		/// <see cref="GrubDied"/> event but only called on the client realm.
		/// </summary>
		[AttributeUsage( AttributeTargets.Method )]
		public class ClientAttribute : EventAttribute
		{
			public ClientAttribute( string? state = null ) : base( ClientEvent + state ?? string.Empty )
			{
				if ( state is null )
					return;

				AssertValidState( state );
			}
		}

		/// <summary>
		/// <see cref="GrubDied"/> event but only called on the server realm.
		/// </summary>
		[AttributeUsage( AttributeTargets.Method )]
		public class ServerAttribute : EventAttribute
		{
			public ServerAttribute( string? state = null ) : base( ServerEvent + state ?? string.Empty )
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
