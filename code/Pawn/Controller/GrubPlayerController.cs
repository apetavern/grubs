using Grubs.Equipment.Weapons;
using Grubs.Gamemodes;
using Grubs.UI.Components;
using Grubs.UI.Inventory;

namespace Grubs.Pawn.Controller;

[Title( "Grubs - Player Controller" ), Category( "Grubs" )]
public sealed partial class GrubPlayerController : Component
{
	[Property] public required GameObject Body { get; set; }
	[Property] public required GameObject Head { get; set; }
	[Property] public required GrubAnimator Animator { get; set; }
	[Property] public required Grub Grub { get; set; }
	[Property] public required GrubCharacterController CharacterController { get; set; }

	[Property] public Vector3 Gravity { get; set; } = new( 0, 0, 800 );
	[Property] public float WishSpeed { get; set; } = 80f;

	public float MouseLookInput => Input.Down( "precision_aim" ) ? Mouse.Delta.y * -0.25f : 0f;
	public float MoveInput => ShouldAcceptInput() ? Input.AnalogMove.y : 0f;

	public float LookInput => ShouldAcceptInput()
		? Input.UsingController ? -Input.GetAnalog( InputAnalog.RightStickY ) : Input.AnalogMove.x + MouseLookInput
		: 0f;

	[Sync] public Angles LookAngles { get; set; }
	[Sync] public int Facing { get; set; } = 1;
	public int LastFacing { get; set; } = 1;
	[Sync] public Rotation EyeRotation { get; set; }
	[Sync] public bool IsChargingBackflip { get; set; }
	[Sync] public float BackflipCharge { get; set; }
	[Sync] public bool IsOnRope { get; set; }
	public bool IsGrounded => CharacterController.IsOnGround;
	public Vector3 Velocity => CharacterController.Velocity;

	protected override void OnUpdate()
	{
		if ( IsProxy )
			return;

		UpdateRotation();
		UpdateLookAngles();
		UpdateEyeRotation();
	}

	protected override void OnFixedUpdate()
	{
		if ( IsProxy )
			return;

		UpdateJump();
		UpdateBackflip();
		UpdateFallVelocity();

		if ( IsGrounded )
		{
			var wishVel = GetWishVelocity();

			CharacterController.Velocity = CharacterController.Velocity.WithZ( 0f );
			CharacterController.Accelerate( wishVel );
			CharacterController.ApplyFriction( 4f, 100f );
		}
		else
		{
			CharacterController.Velocity -= Gravity * Time.Delta * 0.5f;
			CharacterController.ApplyFriction( 0.1f );
		}

		CharacterController.Move();

		if ( !IsGrounded )
			CharacterController.Velocity -= Gravity * Time.Delta * 0.5f;
	}

	private void UpdateRotation()
	{
		if ( CharacterController.Velocity.Normal.IsNearZeroLength )
			return;

		if ( !IsGrounded )
			return;

		Transform.Rotation = MoveInput switch
		{
			<= -1 => Rotation.Identity,
			>= 1 => Rotation.From( 0, 180, 0 ),
			_ => Transform.Rotation
		};
	}

	private void UpdateLookAngles()
	{
		if ( Grub.ActiveEquipment is not null && Grub.ActiveEquipment.Components.TryGet<Weapon>( out var weapon ) )
			if ( weapon.IsCharging )
				return;

		var nextFacing = Transform.Rotation.z <= 0 ? -1 : 1;

		var look = (LookAngles + LookInput * nextFacing).Normal;
		LookAngles = look.WithPitch( look.pitch.Clamp( -80f, 75f ) )
			.WithRoll( 0f )
			.WithYaw( 0f );

		if ( nextFacing != LastFacing )
			LookAngles = LookAngles.WithPitch( LookAngles.pitch * -1 );

		LastFacing = nextFacing;
	}

	private void UpdateEyeRotation()
	{
		Facing = Transform.Rotation.z <= 0 ? 1 : -1;
		EyeRotation = LookAngles.ToRotation();
	}

	private void UpdateJump()
	{
		if ( IsChargingBackflip || !IsGrounded || !ShouldAcceptMoveInput() )
			return;

		if ( Input.Pressed( "jump" ) )
		{
			CharacterController.Velocity = new Vector3( Facing * 175f, 0f, 220f );
			CharacterController.ReleaseFromGround();
			Animator.TriggerJump();
		}
	}

	private void UpdateBackflip()
	{
		if ( !IsGrounded || !ShouldAcceptInput() )
		{
			IsChargingBackflip = false;
			BackflipCharge = 0f;
		}

		if ( !Input.Down( "backflip" ) && IsChargingBackflip )
			DoBackflip();

		if ( Input.Down( "backflip" ) && IsGrounded && ShouldAcceptMoveInput() )
		{
			IsChargingBackflip = true;
			BackflipCharge += 0.02f;
			BackflipCharge = BackflipCharge.Clamp( 0f, 1f );
		}
	}

	private void DoBackflip()
	{
		CharacterController.Velocity =
			new Vector3( -Facing * (25f + 75f * BackflipCharge), 0f, 200f + 220f * BackflipCharge );
		CharacterController.ReleaseFromGround();
		Animator.TriggerBackflip();
		IsChargingBackflip = false;
		BackflipCharge = 0f;
	}

	private Vector3 GetWishVelocity()
	{
		if ( !ShouldAcceptMoveInput() || IsChargingBackflip )
			return 0f;

		var result = new Vector3().WithX( -MoveInput );
		var inSpeed = result.Length.Clamp( 0f, 1f );

		result.z = 0f;

		result = result.Normal * inSpeed;
		result *= WishSpeed;

		return result;
	}

	public bool ShouldShowWeapon()
	{
		var showWhileMoving = false;
		if ( Grub.ActiveEquipment is { } equipment )
		{
			if ( equipment.Ammo == 0 )
				return false;

			if ( equipment.Components.Get<Weapon>() is { } weapon )
			{
				if ( weapon.CanFireWhileMoving )
					showWhileMoving = true;

				if ( weapon.ForceHideWeapon )
					return false;
			}
		}

		return (Velocity.IsNearlyZero( 2.5f ) && IsGrounded || IsOnRope || showWhileMoving) && !IsChargingBackflip;
	}

	/// <summary>
	/// Return
	/// </summary>
	public bool ShouldAcceptMoveInput()
	{
		var equipment = Grub.ActiveEquipment;
		if ( equipment is null )
			return ShouldAcceptInput();
		if ( !equipment.Components.TryGet( out Weapon weapon ) )
			return ShouldAcceptInput();
		return ShouldAcceptInput() && (!weapon.IsFiring || weapon.CanFireWhileMoving);
	}

	public bool ShouldAcceptInput()
	{
		if ( IsProxy || Grub.Player is null )
			return false;

		if ( Gamemode.Current.TurnIsChanging )
			return false;

		if ( !Grub.IsActive )
			return false;

		if ( Input.UsingController && Cursor.IsEnabled() )
			return false;

		if ( PlayerInventory.Local.IsClosing == true )
			return false;

		return true;
	}
}
