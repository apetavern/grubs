using Grubs.States;
using Grubs.UI.World;
using Grubs.Utils;

namespace Grubs.UI;

[UseTemplate]
public class Hud : RootPanel
{
	private WaitingStatus _waitingStatus;
	private TurnTime _turnTime;

	public Hud()
	{
		Event.Register( this );

		if ( Host.IsClient )
		{
			_ = new WormNametags();
		}
	}

	[GrubsEvent.EnterState.Client( nameof( WaitingState ) )]
	private void OnEnterWaiting()
	{
		_waitingStatus?.Delete();
		_waitingStatus = AddChild<WaitingStatus>();
	}

	[GrubsEvent.LeaveState.Client( nameof( WaitingState ) )]
	private void OnLeaveWaiting()
	{
		_waitingStatus?.Delete();
	}

	[GrubsEvent.EnterState.Client( nameof( PlayState ) )]
	private void OnEnterPlay()
	{
		_turnTime?.Delete();
		_turnTime = AddChild<TurnTime>();
	}

	[GrubsEvent.LeaveState.Client( nameof( PlayState ) )]
	private void OnLeavePlay()
	{
		_turnTime?.Delete();
	}
}
