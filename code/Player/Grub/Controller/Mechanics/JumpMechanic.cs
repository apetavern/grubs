namespace Grubs;

public partial class JumpMechanic : ControllerMechanic
{
	public static InputButton JumpButton => InputButton.Jump;
	public override int SortOrder => 5;

	[Net, Predicted]
	public TimeSince TimeSinceJumpPressed { get; set; }

	private static readonly float _timeBeforeSecondPress = 0.25f;
	private bool _backflip = false;
	private bool _firstTime = true;

	private float _jumpPower => 240f;

	protected override bool ShouldStart()
	{
		if ( !Controller.ShouldAllowMovement() )
			return false;

		// If we already pressed jump once, and we didn't hit it again within
		// the time limit, we normal jump.
		if ( TimeSinceJumpPressed > _timeBeforeSecondPress && !_firstTime )
			return true;

		if ( Input.Pressed( JumpButton ) && GroundEntity.IsValid() )
		{
			// Handle the first jump.
			if ( _firstTime )
			{
				_firstTime = false;
				TimeSinceJumpPressed = 0f;
				return false;
			}

			// If Jump is hit twice before the time is up, we want to backflip.
			if ( TimeSinceJumpPressed < _timeBeforeSecondPress )
			{
				_backflip = true;
				return true;
			}
		}

		return false;
	}

	protected override void OnStart()
	{
		if ( _backflip )
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
		Velocity += new Vector3( Grub.Facing * 150f, 0f, 0f );
		Velocity = Velocity.WithZ( _jumpPower );
		Velocity -= new Vector3( 0, 0, Gravity * 0.5f ) * Time.Delta;

		Controller.GetMechanic<SquirmMechanic>()
			.ClearGroundEntity();

		IsActive = false;

		_firstTime = true;
		_backflip = false;
	}

	private void Backflip()
	{
		Velocity += new Vector3( -Grub.Facing * 50f, 0f, 0f );
		Velocity = Velocity.WithZ( _jumpPower * 1.75f );
		Velocity -= new Vector3( 0, 0, Gravity * 0.5f ) * Time.Delta;

		Controller.GetMechanic<SquirmMechanic>()
			.ClearGroundEntity();

		IsActive = false;

		_firstTime = true;
		_backflip = false;
	}
}
