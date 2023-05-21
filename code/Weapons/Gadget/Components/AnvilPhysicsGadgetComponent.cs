namespace Grubs;

[Prefab]
public partial class AnvilPhysicsGadgetComponent : GadgetComponent
{
	[Prefab, Net]
	public bool AffectedByWind { get; set; } = false;

	[Prefab, ResourceType( "sound" )]
	public string CollisionSound { get; set; }

	[Prefab, Net, Predicted]
	public int Bounces { get; set; } = 0;

	[Prefab, Net]
	public float BounceForce { get; set; } = 100.0f;

	[Prefab]
	public bool ShouldExplode { get; set; } = false;

	private bool _isGrounded;
	private bool _wasGrounded;

	public override bool IsResolved()
	{
		return Bounces == 0 && Gadget.Velocity.IsNearlyZero( 2.5f );
	}

	public override void OnUse( Weapon weapon, int charge )
	{
		var trTerrain = Trace.Ray( Player.MousePosition, Player.MousePosition + Vector3.Forward * 1000f )
			.WithAnyTags( "solid" )
			.Size( 1f )
			.Run();

		Gadget.Position = Grub.Player.MousePosition;
		if ( trTerrain.Hit )
			Gadget.Position = Gadget.Position.WithZ( GrubsConfig.TerrainHeight + 64f );
		else
			Gadget.Position += Vector3.Up * 350f;

		Gadget.Position.Clamp( 0, GrubsConfig.TerrainHeight + 64f );
	}

	public override void Simulate( IClient client )
	{
		// Apply gravity.
		Gadget.Velocity -= new Vector3( 0, 0, 400 ) * Time.Delta;

		var helper = new MoveHelper( Gadget.Position, Gadget.Velocity );
		helper.Trace = helper.Trace
			.Size( Gadget.CollisionBounds )
			.WithAnyTags( "player", "solid" )
			.WithoutTags( "dead" );

		helper.TryMove( Time.Delta );
		Gadget.Velocity = helper.Velocity.WithX( 0 );
		Gadget.Position = helper.Position;

		_isGrounded = helper.TraceDirection( Vector3.Down ).Entity is not null;

		if ( _wasGrounded != _isGrounded )
		{
			_wasGrounded = _isGrounded;
			if ( _isGrounded )
			{
				Gadget.PlaySound( CollisionSound );

				if ( ShouldExplode && Gadget.Components.TryGet( out ExplosiveGadgetComponent comp ) )
					comp.Explode();

				HandleBounce();
			}
		}

		if ( GrubsConfig.WindEnabled && AffectedByWind )
			Gadget.Velocity += new Vector3( GamemodeSystem.Instance.ActiveWindForce ).WithY( 0 );
	}

	private void HandleBounce()
	{
		if ( Bounces > 0 )
		{
			Bounces--;
			Gadget.Velocity = Gadget.Velocity.WithZ( Gadget.Velocity.z + BounceForce );
			Gadget.Velocity -= new Vector3( 0, 0, 400f * 0.5f ) * Time.Delta;
		}
		else
		{
			if ( Game.IsServer )
				Gadget.Delete();
		}
	}
}
