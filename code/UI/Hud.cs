using Grubs.Player;
using Grubs.States;
using Grubs.UI.World;
using Grubs.Utils.Event;

namespace Grubs.UI;

public class Hud : RootPanel
{
	private WaitingStatus? _waitingStatus;
	private TurnTime? _turnTime;
	private AimReticle? _aimReticle;
	private readonly List<DamageNumber> _damageNumbers = new();
	private EndScreen? _endScreen;
	private InventoryPanel? _inventoryPanel;

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

	[GrubsEvent.EnterState.Client( nameof( GameEndState ) )]
	private void OnEnterGameEnd()
	{
		_endScreen?.Delete();
		_endScreen = AddChild<EndScreen>();
	}

	[GrubsEvent.LeaveState.Client( nameof( GameEndState ) )]
	private void OnLeaveGameEnd()
	{
		_endScreen?.Delete();
	}

	[GrubsEvent.EnterGamemode.Client]
	private void OnEnterPlay()
	{
		_turnTime?.Delete();
		_turnTime = AddChild<TurnTime>();
		_inventoryPanel = AddChild<InventoryPanel>();

		_aimReticle?.Delete();
		_aimReticle = new AimReticle();
	}

	[GrubsEvent.LeaveGamemode.Client]
	private void OnLeavePlay()
	{
		_turnTime?.Delete();
		_turnTime = null;
		_aimReticle?.Delete();
		_aimReticle = null;
		_inventoryPanel?.Delete();
		foreach ( var damageNumber in _damageNumbers )
			damageNumber.Delete();
		_damageNumbers.Clear();
	}

	[GrubsEvent.GrubHurt.Client]
	private void OnGrubHurt( Grub grub, float damage )
	{
		_damageNumbers.Add( new DamageNumber( grub, damage ) );
	}
}
