namespace Grubs;

public class EnablePointerEvents : Panel
{
	public EnablePointerEvents()
	{
		Event.Register( this );
	}

	[GrubsEvent.Player.PointerEventChanged]
	public void OnPointerEventChanged( bool enabled )
	{
		Style.PointerEvents = enabled ? PointerEvents.All : PointerEvents.None;
	}
}
