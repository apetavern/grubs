namespace Grubs;

[Prefab]
public partial class JetpackComponent : WeaponComponent
{
	[Prefab, Net]
	public float MaxVerticalSpeed { get; set; } = 80f;

	[Prefab, Net]
	public float MaxHorizontalSpeed { get; set; } = 300f;

	[Prefab, Net]
	public float MaxFuel { get; set; } = 15f;

	[Net, Predicted]
	public float RemainingFuel { get; private set; }

	private Particles _leftJetParticle;
	private Particles _rightJetParticle;
	private Particles _backJetParticle;
	private Vector3 _sideJetStrength = Vector3.Zero;
	private Vector3 _backJetStrength = Vector3.Zero;

	private UI.FuelWorldPanel _fuelWorldPanel;

	public override void OnDeploy()
	{
		base.OnDeploy();

		Grub.SetAnimParameter( "jetpack_active", true );

		if ( Game.IsClient )
			_fuelWorldPanel = new( Grub, this );
		else
			RemainingFuel = MaxFuel;

		if ( !Prediction.FirstTime )
			return;

		_leftJetParticle = Particles.Create( "particles/blueflame/blueflame_continuous.vpcf", Weapon, "jet_left" );
		_rightJetParticle = Particles.Create( "particles/blueflame/blueflame_continuous.vpcf", Weapon, "jet_right" );
		_backJetParticle = Particles.Create( "particles/blueflame/blueflame_continuous.vpcf", Weapon, "jet_middle" );
	}

	public override void OnHolster()
	{
		base.OnHolster();

		Grub.SetAnimParameter( "jetpack_active", false );

		// TODO: Rewrite this behaviour.
		if ( RemainingFuel != MaxFuel && Weapon.Ammo > 0 )
			Weapon.Ammo -= 1;

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
		var hasFuel = RemainingFuel > 0;

		_sideJetStrength = _sideJetStrength.LerpTo( isAscending && hasFuel ? new( .7f ) : !controller.IsGrounded && hasFuel ? new( .4f ) : new( 0 ), Time.Delta * 3 );
		_backJetStrength = _backJetStrength.LerpTo( !controller.IsGrounded && hasFuel && horizontalInput != 0 ? new( .65f ) : new( 0 ), Time.Delta * 3 );

		_leftJetParticle.SetPosition( 5, _sideJetStrength );
		_rightJetParticle.SetPosition( 5, _sideJetStrength );
		_backJetParticle.SetPosition( 5, _backJetStrength );

		if ( !hasFuel )
			return;

		if ( !isAscending && controller.IsGrounded )
			return;

		Grub.Controller.ClearGroundEntity();
		Grub.SetAnimParameter( "jetpack_dir", Grub.Facing * horizontalInput );

		var burnRate = 1f * Time.Delta;

		if ( isAscending )
			burnRate *= 1.2f;

		if ( horizontalInput != 0 )
			burnRate *= 1.2f;

		RemainingFuel = Math.Max( RemainingFuel - burnRate, 0f );

		if ( Grub.MoveInput != 0 )
			Grub.Rotation = Grub.MoveInput != 1 ? Rotation.Identity : Rotation.From( 0, 180, 0 );

		var horizontalVelocity = horizontalInput * MaxHorizontalSpeed;
		var verticalVelocity = verticalInput * MaxVerticalSpeed;

		if ( verticalVelocity > 0 )
			controller.Velocity = controller.Velocity.WithZ( verticalVelocity );

		controller.Velocity = Vector3.Lerp( controller.Velocity, controller.Velocity.WithX( horizontalVelocity ), Time.Delta );
		controller.Velocity = controller.Velocity.ComponentMax( controller.Velocity.WithZ( -225 ) );
	}
}
