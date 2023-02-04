namespace Grubs;

public class GrubAnimator : EntityComponent<Grub>
{
	public virtual void Simulate( IClient client )
	{
		var grub = Entity;
		var ctrl = grub.Controller;

		if ( ctrl is null )
			return;

		grub.SetAnimParameter( "grounded", ctrl.IsGrounded );
		grub.SetAnimParameter( "aimangle", grub.EyeRotation.Pitch() );
		grub.SetAnimParameter( "velocity", ctrl.GetWishVelocity().Length );

		if ( grub.ActiveWeapon is not null )
			grub.SetAnimParameter( "holdpose", (int)grub.ActiveWeapon.HoldPose );
	}
}
