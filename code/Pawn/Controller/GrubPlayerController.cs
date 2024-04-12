namespace Grubs.Pawn.Controller;

[Title( "Grubs - Player Controller" ), Category( "Grubs" )]
public sealed partial class GrubPlayerController : Component
{
	[Property] public required GameObject Body { get; set; }
	[Property] public required GameObject Head { get; set; }
	[Property] public required GrubAnimator Animator { get; set; }
	[Property] public required Grub Grub { get; set; }
	[Property] public required GrubCharacterController CharacterController { get; set; }

	[Property] public Vector3 Gravity { get; set; } = new(0, 0, 800);
	[Property] public float WishSpeed { get; set; } = 80f;

	public float MoveInput => Input.AnalogMove.y;
	public float LookInput => Input.AnalogMove.x;
	[Sync] public Angles LookAngles { get; set; }
	[Sync] public int Facing { get; set; } = 1;
	public int LastFacing { get; set; } = 1;
	[Sync] public Rotation EyeRotation { get; set; }
	[Sync] public bool IsChargingBackflip { get; set; }
	[Sync] public float BackflipCharge { get; set; }
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
		if ( IsChargingBackflip || !IsGrounded )
			return;

		if ( Input.Pressed( "jump" ) )
		{
			CharacterController.Velocity = new Vector3( Facing * 175f, 0f, 220f );
			CharacterController.ReleaseFromGround();
		}
	}

	private void UpdateBackflip()
	{
		if ( !IsGrounded )
		{
			IsChargingBackflip = false;
			BackflipCharge = 0f;
		}

		if ( !Input.Down( "backflip" ) && IsChargingBackflip )
			DoBackflip();

		if ( Input.Down( "backflip" ) && IsGrounded )
		{
			IsChargingBackflip = true;
			BackflipCharge += 0.01f;
			BackflipCharge = BackflipCharge.Clamp( 0f, 1f );
		}
	}

	private void DoBackflip()
	{
		CharacterController.Velocity =
			new Vector3( -Facing * (50f + 75f * BackflipCharge), 0f, 150f + 220f * BackflipCharge );
		CharacterController.ReleaseFromGround();

		IsChargingBackflip = false;
		BackflipCharge = 0f;
	}

	private Vector3 GetWishVelocity()
	{
		var result = new Vector3().WithX( -MoveInput );
		var inSpeed = result.Length.Clamp( 0f, 1f );

		result.z = 0f;

		result = result.Normal * inSpeed;
		result *= WishSpeed;

		return result;
	}

	public bool ShouldShowWeapon()
	{
		return Velocity.IsNearlyZero( 2.5f ) && IsGrounded && !IsChargingBackflip;
	}
}
