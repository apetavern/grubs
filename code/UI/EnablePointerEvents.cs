namespace Grubs;

public class EnablePointerEvents : Panel
{
	public EnablePointerEvents()
	{
		Event.Register( this );
	}

	[Event( "pointer.enabled" )]
	public void EnablePointer()
	{
		Log.Info( $"Enabling pointer events for {Game.LocalClient.Name}" );
		Style.PointerEvents = PointerEvents.All;
	}

	[Event( "pointer.disabled" )]
	public void DisablePointer()
	{
		Log.Info( $"Disabling pointer events for {Game.LocalClient.Name}" );
		Style.PointerEvents = PointerEvents.None;
	}
}
