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
		helper.TryMove( Time.Delta );
		Gadget.Position = helper.Position;
		Gadget.Velocity = helper.Velocity;

		if ( LockXAxis )
			Gadget.Velocity = Gadget.Velocity.WithX( 0 );

		helper.Trace = helper.Trace
			.Size( Gadget.CollisionBounds )
			.Ignore( Gadget )
			.WithAnyTags( Tag.Player, Tag.Solid )
			.WithoutTags( Tag.Shard, Tag.Dead );

		_isGrounded = helper.TraceFromTo( Gadget.Position, Gadget.Position ).Hit;

		if ( _isGrounded && Gadget.Components.TryGet( out ExplosiveGadgetComponent comp ) && comp.ExplodeOnTouch )
			comp.Explode();

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
