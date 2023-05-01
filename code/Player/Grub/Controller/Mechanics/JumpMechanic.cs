namespace Grubs;

public partial class JumpMechanic : ControllerMechanic
{
	public override int SortOrder => 5;
	private float _jumpPower => 240f;

	protected override bool ShouldStart()
	{
		if ( !Controller.ShouldAllowMovement() || !Controller.IsGrounded )
			return false;

		if ( Input.Pressed( InputAction.Jump ) )
			return true;

		return false;
	}

	protected override void OnStart()
	{
		Velocity = new Vector3( Grub.Facing * 125f, 0f, _jumpPower );
		Velocity -= new Vector3( 0, 0, Gravity * 0.5f ) * Time.Delta;

		Controller.GetMechanic<SquirmMechanic>()
			.ClearGroundEntity();

		PlayScreenSound( "grub_jump" );
	}
}
