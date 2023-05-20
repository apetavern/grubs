namespace Grubs;

[Prefab]
public partial class PhysicsGadgetComponent : GadgetComponent
{
	[Prefab, Net]
	public bool ShouldRotate { get; set; } = true;

	[Prefab, Net]
	public bool ShouldThrow { get; set; } = false;

	[Prefab, Net]
	public int ThrowSpeed { get; set; } = 10;

	[Prefab, Net]
	public float Friction { get; set; } = 1.0f;

	[Prefab, Net]
	public bool AffectedByWind { get; set; } = false;

	[Prefab, Net]
	public bool CheckResolve { get; set; } = true;

	[Prefab, ResourceType( "sound" )]
	public string CollisionSound { get; set; }

	private bool _isGrounded;
	private bool _lastKnownGround;

	public override void OnUse( Weapon weapon, int charge )
	{
		if ( ShouldThrow )
		{
			Gadget.Position = weapon.GetStartPosition();
			Gadget.Velocity = Grub.EyeRotation.Forward.Normal * Grub.Facing * charge * ThrowSpeed;
		}
	}

	public override bool IsResolved()
	{
		return CheckResolve ? Gadget.Velocity.IsNearlyZero( 2.5f ) : true;
	}

	public override void Simulate( IClient client )
	{
		// Apply gravity.
		Gadget.Velocity -= new Vector3( 0, 0, 400 ) * Time.Delta;

		var helper = new MoveHelper( Gadget.Position, Gadget.Velocity );
		helper.Trace = helper.Trace
			.Size( Gadget.CollisionBounds )
			.Ignore( Grub )
			.WithAnyTags( "player", "solid" )
			.WithoutTags( "dead" );

		_isGrounded = helper.TraceDirection( Vector3.Down ).Entity is not null;

		if ( _isGrounded )
			helper.ApplyFriction( Friction, Time.Delta );

		if ( _lastKnownGround != _isGrounded || helper.HitWall )
		{
			_lastKnownGround = _isGrounded || helper.HitWall;

			if ( _lastKnownGround )
				Gadget.PlaySound( CollisionSound );
		}

		helper.TryMove( Time.Delta );
		Gadget.Velocity = helper.Velocity;
		Gadget.Position = helper.Position;

		if ( GrubsConfig.WindEnabled && AffectedByWind )
			Gadget.Velocity += new Vector3( GamemodeSystem.Instance.ActiveWindForce ).WithY( 0 );

		if ( ShouldRotate )
		{
			// Apply rotation using some shit I pulled out of my ass.
			var angularX = Gadget.Velocity.x * 5f * Time.Delta;
			float degrees = angularX.Clamp( -20, 20 );
			Gadget.Rotation = Gadget.Rotation.RotateAroundAxis( new Vector3( 0, 1, 0 ), degrees );
		}
		else
		{
			Gadget.Rotation = Rotation.Identity;
		}
	}
}
