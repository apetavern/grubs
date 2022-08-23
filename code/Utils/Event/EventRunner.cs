namespace Grubs.Utils.Event;

public partial class EventRunner : Entity
{
	private static EventRunner _instance;

	public EventRunner()
	{
		_instance = this;
	}

	public static void Run( string name, To? to = null )
	{
		RunLocal( name );

		if ( Host.IsServer )
			_instance.RunRpc( to ?? To.Everyone, name );
	}

	public static void RunLocal( string name )
	{
		Sandbox.Event.Run( name );
		Sandbox.Event.Run( GetRealmPrefix() + name );
	}

	public static void RunLocal<T1>( string name, T1 arg0 )
	{
		Sandbox.Event.Run( name, arg0 );
		Sandbox.Event.Run( GetRealmPrefix() + name, arg0 );
	}

	public static void RunLocal<T1, T2>( string name, T1 arg0, T2 arg1 )
	{
		Sandbox.Event.Run( name, arg0, arg1 );
		Sandbox.Event.Run( GetRealmPrefix() + name, arg0, arg1 );
	}

	[ClientRpc]
	private void RunRpc( string name )
	{
		RunLocal( name );
	}

	private static string GetRealmPrefix()
	{
		return Host.IsServer ? GrubsEvent.ServerPrefix : GrubsEvent.ClientPrefix;
	}
}
