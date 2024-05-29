using Grubs;
using Grubs.Equipment;
using Grubs.Pawn;
using Sandbox;
using Sandbox.UI;
using System.Numerics;
using System;
using System.Threading.Tasks;
using Grubs.Equipment.Weapons;

namespace Grubs.Bots;

public partial class BotBrain : Component
{
	[Property] public Grub ActiveGrub { get; set; }

	private BotState currentState;
	private Grub targetGrub;
	private Weapon selectedWeapon;
	private Vector3 targetPosition;
	TimeSince TimeInState;

	protected override void OnStart()
	{
		base.OnStart();
		currentState = BotState.Idle;
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		switch ( currentState )
		{
			case BotState.Idle:
				OnIdle();
				break;
			case BotState.Targeting:
				OnTargeting();
				break;
			case BotState.Moving:
				OnMoving();
				break;
			case BotState.SelectingWeapon:
				OnSelectingWeapon();
				break;
			case BotState.Aiming:
				OnAiming();
				break;
			case BotState.Firing:
				OnFiring();
				break;
			case BotState.Cooldown:
				OnCooldown();
				break;
		}

		if ( TimeInState > GrubsConfig.TurnDuration / 4f )
		{
			ForceNextState();
		}
	}

	private void ForceNextState()
	{
		currentState++;
		TimeInState = 0f;
	}
}

public enum BotState
{
	Idle,
	Targeting,
	Moving,
	SelectingWeapon,
	Aiming,
	Firing,
	Cooldown
}
