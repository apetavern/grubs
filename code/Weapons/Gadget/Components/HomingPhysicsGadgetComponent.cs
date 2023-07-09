namespace Grubs;

[Prefab]
public partial class HomingPhysicsGadgetComponent : ArcPhysicsGadgetComponent
{
	[Prefab, ResourceType( "sound" )]
	public string LockOnSound { get; set; }

	[Net, Prefab]
	private float TimeUntilHoming { get; set; } = 0.6f;

	[Net, Prefab]
	private float TimeUntilFail { get; set; } = 3f;

	[Net]
	private Vector3 TargetPosition { get; set; }

	[Net]
	private TimeSince TimeSinceFired { get; set; }

	private bool _isHoming = false;
	private bool _isInitialized = false;

	public override void OnUse( Weapon weapon, int charge )
	{
		base.OnUse( weapon, charge );

		TargetPosition = weapon.Components.Get<GadgetWeaponComponent>().TargetPreview.Position.WithY( 0f );
		TimeSinceFired = 0f;
	}

	public override void Simulate( IClient client )
	{
		if ( TimeSinceFired >= TimeUntilHoming && !_isInitialized )
		{
			_isHoming = true;
			_isInitialized = true;
			Gadget.Velocity = Gadget.Rotation.Forward * ProjectileSpeed / 2f;
			Gadget.PlaySound( LockOnSound );
		}

		if ( _isHoming )
			RunTowardsTarget();
		else
			RunAlongSegments();
	}

	private void RunTowardsTarget()
	{
		var rotation = Rotation.LookAt( (TargetPosition - Gadget.Position).WithY( 0 ), Vector3.Right );

		Gadget.Rotation = Rotation.Slerp( Gadget.Rotation, rotation, 0.075f );
		Gadget.Velocity = Vector3.Lerp( Gadget.Velocity, Gadget.Rotation.Forward * ProjectileSpeed / 4f, 0.5f );
		Gadget.Position += Gadget.Velocity;
		Gadget.Position = Gadget.Position.WithY( 0 );

		if ( TimeSinceFired > TimeUntilFail )
		{
			_isHoming = false;
			Segments = CalculateTrajectory( Gadget.Velocity, (int)(ProjectileSpeed / 20f) );
			return;
		}

		if ( Trace.Ray( Gadget.Position, Gadget.Position + Gadget.Velocity ).Size( Gadget.CollisionBounds ).Ignore( Gadget ).Run().Hit )
			_explosiveComponent?.ExplodeAfterSeconds( _explosiveComponent.ExplodeAfter );
	}
}
