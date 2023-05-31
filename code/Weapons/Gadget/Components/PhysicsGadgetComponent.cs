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

	[Prefab, Net]
	public bool ShouldBounce { get; set; } = false;

	[Prefab, Net]
	public bool AutoMove { get; set; } = false;

	[Prefab, Net]
	public bool IsAnimated { get; set; } = false;

	[Prefab, ResourceType( "sound" )]
	public string CollisionSound { get; set; }

	private bool _isGrounded;
	private bool _wasGrounded;
	private bool _isTouchingCeiling;
	private bool _wasTouchingCeiling;

	[Net]
	private float _moveDirection { get; set; }

	public override void OnUse( Weapon weapon, int charge )
	{
		if ( ShouldThrow )
		{
			Gadget.Position = weapon.GetStartPosition();
			Gadget.Velocity = Grub.EyeRotation.Forward.Normal * Grub.Facing * charge * ThrowSpeed;
		}

		if ( AutoMove )
			_moveDirection = Grub.Facing;
	}

	public override bool IsResolved()
	{
		return CheckResolve ? Gadget.Velocity.IsNearlyZero( 2.5f ) : true;
	}

	public override void Simulate( IClient client )
	{
		// Apply gravity.
		Gadget.Velocity -= new Vector3( 0, 0, 400 ) * Time.Delta;

		if ( ShouldBounce && _isGrounded && !_isTouchingCeiling )
			Gadget.Velocity = Vector3.Up * 250f * Game.Random.Float( 0.5f, 1.5f );

		var helper = new MoveHelper( Gadget.Position, Gadget.Velocity );
		helper.Trace = helper.Trace
			.Size( Gadget.CollisionBounds )
			.Ignore( Grub )
			.WithAnyTags( Tag.Player, Tag.Solid )
			.WithoutTags( Tag.Dead );

		_isGrounded = helper.TraceDirection( Vector3.Down ).Entity is not null;
		_isTouchingCeiling = helper.TraceDirection( Vector3.Up ).Entity is not null;

		if ( IsAnimated )
		{
			Gadget.SetAnimParameter( "active", true );
			Gadget.SetAnimParameter( "grounded", _isGrounded );
		}

		if ( _isGrounded )
			helper.ApplyFriction( Friction, Time.Delta );

		if ( _wasGrounded != _isGrounded || _wasTouchingCeiling != _isTouchingCeiling || helper.HitWall )
		{
			_wasGrounded = _isGrounded || helper.HitWall;
			_wasTouchingCeiling = _isTouchingCeiling;

			if ( _wasGrounded || _wasTouchingCeiling )
				OnCollision();
		}

		helper.TryMove( Time.Delta );
		Gadget.Velocity = helper.Velocity;
		Gadget.Position = helper.Position;

		if ( AutoMove )
			Gadget.Velocity = Gadget.Velocity.WithX( _moveDirection * ThrowSpeed );

		if ( GrubsConfig.WindEnabled && AffectedByWind )
			Gadget.Velocity += new Vector3( GamemodeSystem.Instance.ActiveWindForce ).WithY( 0 );

		if ( ShouldRotate )
		{
			// Apply rotation using some shit I pulled out of my ass.
			var angularX = Gadget.Velocity.x * 5f * Time.Delta;
			float degrees = angularX.Clamp( -20, 20 );
			Gadget.Rotation = Gadget.Rotation.RotateAroundAxis( new Vector3( 0, 1, 0 ), degrees );
		}
		else if ( IsAnimated )
		{
			var pitch = MathX.Lerp( Gadget.Rotation.Pitch(), Gadget.Velocity.z / 15f, 0.25f );
			if ( _moveDirection == 1 )
				Gadget.Rotation = Rotation.From( pitch, 0, 0 );
			else if ( _moveDirection == -1 )
				Gadget.Rotation = Rotation.From( pitch, 180, 0 );
		}
		else
		{
			Gadget.Rotation = Rotation.Identity;
		}
	}

	private void OnCollision()
	{
		Gadget.PlaySound( CollisionSound );

		if ( Gadget.Components.TryGet( out ExplosiveGadgetComponent comp ) && comp.ExplodeOnTouch )
			comp.Explode();
	}
}
