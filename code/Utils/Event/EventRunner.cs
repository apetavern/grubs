namespace Grubs.Utils.Event;

/// <summary>
/// Utility <see cref="Entity"/> to run events across both realms and simplify event execution.
/// </summary>
[Category( "Setup" )]
public sealed partial class EventRunner : Entity
{
	private static EventRunner _instance = null!;

	public EventRunner()
	{
		_instance = this;
	}

	/// <summary>
	/// Runs a basic event on both server and client.
	/// </summary>
	/// <param name="name">The name of the event to run.</param>
	/// <param name="to">The <see cref="IClient"/>s to send this event to.</param>
	public static void Run( string name, To? to = null )
	{
		RunLocal( name );

		if ( Game.IsServer )
			_instance.RunRpc( to ?? To.Everyone, name );
	}

	/// <summary>
	/// Runs a basic event on the local realm.
	/// </summary>
	/// <param name="name">The name of the event to run.</param>
	public static void RunLocal( string name )
	{
		Sandbox.Event.Run( name );
		Sandbox.Event.Run( GetRealmPrefix() + name );
	}

	/// <summary>
	/// Runs an event on the local realm.
	/// </summary>
	/// <param name="name">The name of the event to run.</param>
	/// <param name="arg0">The first argument to send to the event listeners.</param>
	/// <typeparam name="T1">The type of <see cref="arg0"/>.</typeparam>
	public static void RunLocal<T1>( string name, T1 arg0 )
	{
		Sandbox.Event.Run( name, arg0 );
		Sandbox.Event.Run( GetRealmPrefix() + name, arg0 );
	}

	/// <summary>
	/// Runs an event on the local realm.
	/// </summary>
	/// <param name="name">The name of the event to run.</param>
	/// <param name="arg0">The first argument to send to the event listeners.</param>
	/// <param name="arg1">The second argument to send to the event listeners.</param>
	/// <typeparam name="T1">The type of <see cref="arg0"/>.</typeparam>
	/// <typeparam name="T2">The type of <see cref="arg1"/>.</typeparam>
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

	/// <summary>
	/// Gets the prefix that this realm is in.
	/// </summary>
	/// <returns>The prefix of the current realm.</returns>
	private static string GetRealmPrefix()
	{
		return Game.IsServer ? GrubsEvent.ServerPrefix : GrubsEvent.ClientPrefix;
	}
}
