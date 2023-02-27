namespace Grubs;

public partial class JumpMechanic : ControllerMechanic
{
	public static InputButton JumpButton => InputButton.Jump;
	public static InputButton BackflipButton => InputButton.Run;
	public override int SortOrder => 5;

	[Net, Predicted]
	public bool IsBackflipping { get; set; } = false;

	private static float _jumpPower => 240f;

	protected override bool ShouldStart()
	{
		if ( !Controller.ShouldAllowMovement() )
			return false;

		if ( Controller.IsGrounded )
		{
			if ( Input.Pressed( JumpButton ) )
			{
				IsBackflipping = false;
				return true;
			}

			if ( Input.Pressed( BackflipButton ) )
			{
				IsBackflipping = true;
				return true;
			}
		}

		return false;
	}

	protected override void OnStart()
	{
		if ( IsBackflipping )
		{
			Backflip();
		}
		else
		{
			Jump();
		}
	}

	private void Jump()
	{
		Velocity = new Vector3( Grub.Facing * 125f, 0f, _jumpPower );
		Velocity -= new Vector3( 0, 0, Gravity * 0.5f ) * Time.Delta;

		Controller.GetMechanic<SquirmMechanic>()
			.ClearGroundEntity();

		IsActive = false;
		IsBackflipping = false;
	}

	private void Backflip()
	{
		Grub.Animator.Backflip();

		Velocity += new Vector3( -Grub.Facing * 50f, 0f, 0f );
		Velocity = Velocity.WithZ( _jumpPower * 1.75f );
		Velocity -= new Vector3( 0, 0, Gravity * 0.5f ) * Time.Delta;

		Controller.GetMechanic<SquirmMechanic>()
			.ClearGroundEntity();

		IsActive = false;
		IsBackflipping = false;
	}
}
