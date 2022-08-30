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
	private AimReticle? _aimReticle;
	private List<DamageNumber> _damageNumbers = new();

	public Hud()
	{
		Event.Register( this );

		if ( Host.IsClient )
		{
			_ = new GrubNametags();
			AddChild<ChatBox>();
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

	[GrubsEvent.EnterGamemode.Client]
	private void OnEnterPlay()
	{
		_turnTime?.Delete();
		_turnTime = AddChild<TurnTime>();

		_aimReticle?.Delete();
		_aimReticle = new AimReticle();
	}

	[GrubsEvent.LeaveGamemode.Client]
	private void OnLeavePlay()
	{
		_turnTime?.Delete();
		_aimReticle?.Delete();
		foreach ( var damageNumber in _damageNumbers )
			damageNumber.Delete();
	}

	[GrubsEvent.GrubHurt.Client]
	private void OnGrubHurt( Grub grub, float damage )
	{
		_damageNumbers.Add( new DamageNumber( grub, damage ) );
	}
}
