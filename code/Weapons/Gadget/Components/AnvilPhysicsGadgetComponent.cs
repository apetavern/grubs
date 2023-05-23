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

	[Prefab, Net, Predicted]
	public int Bounces { get; set; } = 0;

	[Prefab, Net]
	public float BounceForce { get; set; } = 100.0f;

	private bool _isGrounded;
	private bool _wasGrounded;

	public override bool IsResolved()
	{
		return Bounces == 0 && Gadget.Velocity.IsNearlyZero( 2.5f );
	}

	public override void OnUse( Weapon weapon, int charge )
	{
		int zPos = GrubsConfig.WorldTerrainType is GrubsConfig.TerrainType.Texture ? GrubsGame.Instance.Terrain.WorldTextureHeight : GrubsConfig.TerrainHeight;
		Gadget.Position = weapon.GetStartPosition().WithZ( zPos + 192f );
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

		var trCollision = Trace.Ray( Gadget.Position, Gadget.Position )
				.Size( Gadget.CollisionBounds * 1.1f ) // Slightly increase to make sure it collides.
				.Ignore( Gadget )
				.WithAnyTags( Tag.Player, Tag.Solid )
				.WithoutTags( Tag.Shard )
				.UseHitboxes( true )
				.Run();

		_isGrounded = trCollision.Hit;

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

		if ( Bounces > 0 )
		{
			Bounces--;
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
