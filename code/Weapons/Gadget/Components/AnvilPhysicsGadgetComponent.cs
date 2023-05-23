namespace Grubs;

[Prefab]
public partial class AnvilPhysicsGadgetComponent : GadgetComponent
{
	[Prefab, Net]
	public bool LockXAxis { get; set; } = true;

	[Prefab, Net]
	public bool AffectedByWind { get; set; } = false;

	[Prefab, ResourceType( "sound" )]
	public string CollisionSound { get; set; }

	[Prefab, Net]
	public float BounceForce { get; set; } = 100.0f;

	[Prefab, Net]
	public int MaxBounces { get; set; } = 0;

	[Net, Predicted]
	private int _bouncesRemaining { get; set; }

	private bool _isGrounded;
	private bool _wasGrounded;

	public override bool IsResolved()
	{
		return _bouncesRemaining == 0 && Gadget.Velocity.IsNearlyZero( 2.5f );
	}

	public override void OnUse( Weapon weapon, int charge )
	{
		_bouncesRemaining = MaxBounces;

		Gadget.Position = Gadget.Position.WithZ( GrubsGame.Instance.Terrain.WorldTextureHeight + 256f );
	}

	public override void Simulate( IClient client )
	{
		// Apply gravity.
		Gadget.Velocity -= new Vector3( 0, 0, 400 ) * Time.Delta;

		var helper = new MoveHelper( Gadget.Position, Gadget.Velocity );
		helper.Trace = helper.Trace
			.Size( Gadget.CollisionBounds )
			.WithAnyTags( Tag.Player, Tag.Solid )
			.WithoutTags( Tag.Dead )
			.UseHitboxes( true );

		helper.TryMove( Time.Delta );
		Gadget.Position = helper.Position;
		Gadget.Velocity = helper.Velocity;
		if ( LockXAxis )
			Gadget.Velocity = Gadget.Velocity.WithX( 0 );

		// Update collision trace to use same CollisionBounds as ExplosiveGadgetComponent.
		// I don't know why we can't just set this in the original trace,
		// but testing shows that the gadget will just bounce on the ground and never explode.
		helper.Trace = helper.Trace
			.Size( Gadget.CollisionBounds * 1.1f );

		_isGrounded = helper.TraceFromTo( Gadget.Position, Gadget.Position ).Hit;

		if ( _wasGrounded != _isGrounded )
		{
			_wasGrounded = _isGrounded;
			if ( _isGrounded )
				OnCollision();
		}

		if ( GrubsConfig.WindEnabled && AffectedByWind )
			Gadget.Velocity += new Vector3( GamemodeSystem.Instance.ActiveWindForce ).WithY( 0 );
	}

	private void OnCollision()
	{
		Gadget.PlaySound( CollisionSound );

		if ( _bouncesRemaining > 0 )
		{
			_bouncesRemaining--;
			Gadget.Velocity = Gadget.Velocity.WithZ( BounceForce );
			Gadget.Velocity -= new Vector3( 0, 0, 400f * 0.5f ) * Time.Delta;
		}
		else
		{
			if ( Game.IsServer )
				Gadget.Delete();
		}
	}
}
