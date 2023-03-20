namespace Grubs;

public class GrubAnimator : EntityComponent<Grub>
{
	private float _incline;
	private TimeSince _timeSinceBackflip;

	public void Backflip()
	{
		_timeSinceBackflip = 0f;
	}

	public virtual void Simulate( IClient client )
	{
		var grub = Entity;
		var ctrl = grub.Controller;

		if ( ctrl is null )
			return;

		grub.SetAnimParameter( "backflip", _timeSinceBackflip < 0.1f );
		grub.SetAnimParameter( "grounded", ctrl.IsGrounded );
		grub.SetAnimParameter( "aimangle", grub.EyeRotation.Pitch() * -grub.Facing );
		grub.SetAnimParameter( "velocity", ctrl.GetWishVelocity().Length );
		grub.SetAnimParameter( "lowhp", grub.Health <= 20f );
		grub.SetAnimParameter( "explode", grub.LifeState == LifeState.Dying );

		var airMove = ctrl.GetMechanic<AirMoveMechanic>();
		var isHardFalling = false;
		if ( airMove is not null )
			isHardFalling = airMove.IsHardFalling;

		grub.SetAnimParameter( "hardfall", isHardFalling );
		grub.SetAnimParameter( "sliding", grub.HasBeenDamaged && !isHardFalling && !ctrl.Velocity.IsNearlyZero( 2.5f ) );

		var tr = Trace.Ray( grub.Position + grub.Rotation.Up * 10f, grub.Position + grub.Rotation.Down * 128 )
			.Size( 2f )
			.Ignore( grub )
			.WithoutTags( "trigger" )
			.IncludeClientside()
			.Run();
		_incline = MathX.Lerp( _incline, grub.Rotation.Forward.Angle( tr.Normal ) - 90f, 0.25f );
		grub.SetAnimParameter( "incline", _incline );
		grub.SetAnimParameter( "heightdiff", tr.Distance );

		var holdPose = HoldPose.None;
		if ( grub.IsTurn && grub.ActiveWeapon is not null && ctrl.ShouldShowWeapon() )
			holdPose = grub.ActiveWeapon.HoldPose;
		grub.SetAnimParameter( "holdpose", (int)holdPose );
	}
}
