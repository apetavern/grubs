namespace Grubs;

public partial class BackflipMechanic : ControllerMechanic
{
	public override int SortOrder => 5;
	private float _jumpPower => 240f;

	protected override bool ShouldStart()
	{
		if ( !Controller.ShouldAllowMovement() || !Controller.IsGrounded )
			return false;

		if ( Input.Pressed( InputAction.Backflip ) )
			return true;

		return false;
	}

	protected override void OnStart()
	{
		Velocity += new Vector3( -Grub.Facing * 50f, 0f, 0f );
		Velocity = Velocity.WithZ( _jumpPower * 1.75f );
		Velocity -= new Vector3( 0, 0, Gravity * 0.5f ) * Time.Delta;

		Controller.GetMechanic<SquirmMechanic>()
			.ClearGroundEntity();

		if ( Game.IsClient )
			Grub.SoundFromScreen( "grub_backflip" );
	}
}
