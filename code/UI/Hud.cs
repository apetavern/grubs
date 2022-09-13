using Grubs.Player;
using Grubs.States;
using Grubs.UI.World;
using Grubs.Utils.Event;

namespace Grubs.UI;

public sealed class Hud : RootPanel
{
	private WaitingStatus? _waitingStatus;
	private TurnTime? _turnTime;
	private InventoryPanel? _inventoryPanel;
	private AimReticle? _aimReticle;
	private readonly List<DamageNumber> _damageNumbers = new();
	private EndScreen? _endScreen;

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

		_inventoryPanel?.Delete();
		_inventoryPanel = AddChild<InventoryPanel>();

		_aimReticle?.Delete();
		_aimReticle = new AimReticle();
	}

	[GrubsEvent.LeaveGamemode.Client]
	private void OnLeavePlay()
	{
		_turnTime?.Delete();
		_turnTime = null;
		_inventoryPanel?.Delete();
		_inventoryPanel = null;
		_aimReticle?.Delete();
		_aimReticle = null;

		foreach ( var damageNumber in _damageNumbers )
			damageNumber.Delete();
		_damageNumbers.Clear();
	}

	[GrubsEvent.GrubHealed.Client]
	private void OnGrubHealed( Grub grub, float damage )
	{
		_damageNumbers.Add( new DamageNumber( grub, -damage ) );
	}

	[GrubsEvent.GrubHurt.Client]
	private void OnGrubHurt( Grub grub, float damage )
	{
		_damageNumbers.Add( new DamageNumber( grub, damage ) );
	}
}
