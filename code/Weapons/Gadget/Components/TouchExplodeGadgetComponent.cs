namespace Grubs;

[Prefab]
public partial class TouchExplodeGadgetComponent : GadgetComponent
{
	public override void Spawn()
	{
		Gadget.Tags.Add( "shard" );
		Gadget.EnableTouchPersists = true;
	}

	public override void Simulate( IClient client )
	{
		base.Simulate( client );
		var tr = Trace.Ray( Gadget.Position, Gadget.Position + Gadget.Velocity.Normal * 4f ).Size( 20f ).WithoutTags( "shard" ).Run();
		if ( (tr.Hit && tr.Entity.GetType() != typeof( Gadget )) )
		{
			Gadget.Components.Get<ExplosiveGadgetComponent>()?.Explode();
		}
	}
}
