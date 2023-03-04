namespace Grubs;

[Prefab]
public partial class PhysicsGadgetComponent : GadgetComponent
{
	[Prefab, Net]
	public bool ShouldRotate { get; set; } = true;

	public override void Simulate( IClient client )
	{
		// Apply gravity.
		Gadget.Velocity -= new Vector3( 0, 0, 400 ) * Time.Delta;

		var helper = new MoveHelper( Gadget.Position, Gadget.Velocity );
		helper.Trace = helper.Trace.Size( 12f ).WithAnyTags( "player", "solid" ).WithoutTags( "dead" ).Ignore( Gadget );
		helper.TryMove( Time.Delta );
		Gadget.Velocity = helper.Velocity;
		Gadget.Position = helper.Position;

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
