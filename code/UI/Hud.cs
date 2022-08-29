using Grubs.Player;
using Grubs.States;
using Grubs.UI.World;
using Grubs.Utils.Event;

namespace Grubs.UI;

[UseTemplate]
public class Hud : RootPanel
{
	private WaitingStatus? _waitingStatus;
	private TurnTime? _turnTime;
	private List<DamageNumber> _damageNumbers = new();

	public Hud()
	{
		Event.Register( this );

		if ( Host.IsClient )
		{
			_ = new GrubNametags();
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

	[GrubsEvent.EnterState.Client( nameof( FreeForAll ) )]
	private void OnEnterPlay()
	{
		_turnTime?.Delete();
		_turnTime = AddChild<TurnTime>();
	}

	[GrubsEvent.LeaveState.Client( nameof( FreeForAll ) )]
	private void OnLeavePlay()
	{
		_turnTime?.Delete();
		foreach ( var damageNumber in _damageNumbers )
			damageNumber.Delete();
	}

	[GrubsEvent.GrubHurt.Client]
	private void OnGrubHurt( Grub grub, float damage )
	{
		_damageNumbers.Add( new DamageNumber( grub, damage ) );
	}
}
