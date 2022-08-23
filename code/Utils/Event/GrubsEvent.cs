using Grubs.States;

namespace Grubs.Utils.Event;

public static class GrubsEvent
{
	public const string ClientPrefix = "Client.";
	public const string ServerPrefix = "Server.";

	#region EnterState
	public const string EnterStateEvent = "grubs_enter-state";

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

	public static class EnterState
	{
		public const string ClientEvent = ClientPrefix + EnterStateEvent;
		public const string ServerEvent = ServerPrefix + EnterStateEvent;

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
	public const string LeaveStateEvent = "grubs_leave-state";

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

	public static class LeaveState
	{
		public const string ClientEvent = ClientPrefix + LeaveStateEvent;
		public const string ServerEvent = ServerPrefix + LeaveStateEvent;

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
	public const string WormHurtEvent = "grubs_worm-hurt";

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

	public static class WormHurt
	{
		public const string ClientEvent = ClientPrefix + WormHurtEvent;
		public const string ServerEvent = ServerPrefix + WormHurtEvent;

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

	private static void AssertValidState( string state )
	{
		//var type = TypeLibrary.GetTypeByName( state );
		//Assert.True( type is not null && type.IsAssignableTo( typeof(BaseState) ) );
	}
}
