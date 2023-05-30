namespace Grubs;

[Prefab]
public partial class JetpackComponent : WeaponComponent
{
	[Prefab]
	public float MaxVerticalSpeed { get; set; } = 80f;

	[Prefab]
	public float MaxHorizontalSpeed { get; set; } = 300f;

	private Particles _leftJetParticle;
	private Particles _rightJetParticle;
	private Particles _bottomJetParticle;

	public override void OnDeploy()
	{
		base.OnDeploy();

		Grub.SetAnimParameter( "jetpack_active", true );

		if ( !Prediction.FirstTime )
			return;

		// _leftJetParticle = Particles.Create( "particles/blueflame/blueflame_continuous.vpcf" );
		// _rightJetParticle = Particles.Create( "particles/blueflame/blueflame_continuous.vpcf" );
		// _bottomJetParticle = Particles.Create( "particles/blueflame/blueflame_continuous.vpcf" );

		// _leftJetParticle.SetEntityAttachment( 0, Weapon, "jet_middle" );
		// _rightJetParticle.SetEntityAttachment( 0, Weapon, "jet_left" );
		// _bottomJetParticle.SetEntityAttachment( 0, Weapon, "jet_right" );
	}

	public override void OnHolster()
	{
		base.OnHolster();

		Grub.SetAnimParameter( "jetpack_active", false );

		// _leftJetParticle?.Destroy( true );
		// _rightJetParticle?.Destroy( true );
		// _bottomJetParticle?.Destroy( true );
	}

	public override void Simulate( IClient client )
	{
		base.Simulate( client );

		var controller = Grub.Controller;
		var horizontalInput = -Grub.MoveInput;
		var verticalInput = -Grub.LookInput * Grub.Facing;

		var isAscending = verticalInput > 0;

		if ( !isAscending && controller.IsGrounded )
			return;

		Grub.Controller.ClearGroundEntity();
		Grub.SetAnimParameter( "jetpack_dir", Grub.Facing * horizontalInput );

		if ( Grub.MoveInput != 0 )
			Grub.Rotation = Grub.MoveInput != 1 ? Rotation.Identity : Rotation.From( 0, 180, 0 );

		var horizontalVelocity = horizontalInput * MaxHorizontalSpeed;
		var verticalVelocity = verticalInput * MaxVerticalSpeed;

		if ( verticalVelocity > 0 )
			controller.Velocity = controller.Velocity.WithZ( verticalVelocity );

		controller.Velocity = Vector3.Lerp( controller.Velocity, controller.Velocity.WithX( horizontalVelocity ), Time.Delta );
		controller.Velocity = controller.Velocity.ComponentMax( controller.Velocity.WithZ( -350 ) );
	}
}
