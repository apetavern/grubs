namespace Grubs;

[Prefab]
public partial class JetpackComponent : WeaponComponent
{
	[Prefab, Net]
	public float MaxVerticalSpeed { get; set; } = 80f;

	[Prefab, Net]
	public float MaxHorizontalSpeed { get; set; } = 300f;

	[Prefab, Net]
	public float MaxFuel { get; set; } = 25f;

	[Net, Predicted]
	public float FuelRemaining { get; private set; }

	private Particles _leftJetParticle;
	private Particles _rightJetParticle;
	private Particles _backJetParticle;

	private UI.FuelWorldPanel _fuelWorldPanel;

	public override void OnDeploy()
	{
		base.OnDeploy();

		Grub.SetAnimParameter( "jetpack_active", true );

		if ( Game.IsClient )
			_fuelWorldPanel = new( Grub, this );
		else
			FuelRemaining = MaxFuel;

		if ( !Prediction.FirstTime )
			return;

		_leftJetParticle = Particles.Create( "particles/blueflame/blueflame_continuous.vpcf" );
		_rightJetParticle = Particles.Create( "particles/blueflame/blueflame_continuous.vpcf" );
		_backJetParticle = Particles.Create( "particles/blueflame/blueflame_continuous.vpcf" );

		_leftJetParticle.SetEntityAttachment( 0, Weapon, "jet_left" );
		_rightJetParticle.SetEntityAttachment( 0, Weapon, "jet_right" );
		_backJetParticle.SetEntityAttachment( 0, Weapon, "jet_middle" );
	}

	public override void OnHolster()
	{
		base.OnHolster();

		Grub.SetAnimParameter( "jetpack_active", false );

		if ( Game.IsClient )
			_fuelWorldPanel.Delete( true );

		_leftJetParticle?.Destroy( true );
		_rightJetParticle?.Destroy( true );
		_backJetParticle?.Destroy( true );
	}

	public override void Simulate( IClient client )
	{
		base.Simulate( client );

		var controller = Grub.Controller;
		var horizontalInput = -Grub.MoveInput;
		var verticalInput = -Grub.LookInput * Grub.Facing;

		var isAscending = verticalInput > 0;

		_leftJetParticle.EnableDrawing = isAscending && !controller.IsGrounded;
		_rightJetParticle.EnableDrawing = isAscending && !controller.IsGrounded;
		_backJetParticle.EnableDrawing = horizontalInput != 0 && !controller.IsGrounded;

		if ( !isAscending && controller.IsGrounded )
			return;

		Grub.Controller.ClearGroundEntity();
		Grub.SetAnimParameter( "jetpack_dir", Grub.Facing * horizontalInput );

		var isUsingJetpack = isAscending || horizontalInput != 0;
		if ( isUsingJetpack )
			FuelRemaining -= Time.Delta;

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
